using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Navigator.Primitives;

namespace Navigator.Droid.UIElements
{
    public class CustomImageViewGestureDetector : GestureDetector.SimpleOnGestureListener
    {
        private readonly CustomImageView _imgView;

        public CustomImageViewGestureDetector(CustomImageView imageView)
        {
            _imgView = imageView;
        }

        public override bool OnDown(MotionEvent e)
        {
            return true;
        }

        public override bool OnDoubleTap(MotionEvent e)
        {
            _imgView.MaxZoomTo((int) e.GetX(), (int) e.GetY());
            _imgView.PostTransitionCutting();
            return true;
        }

        public override void OnLongPress(MotionEvent e)
        {
            _imgView.OnLongPress(e);
        }
    }

    public delegate void LongPressHandler(object sender, MotionEvent e);

    public class CustomImageView : ImageView, View.IOnTouchListener
    {
        /// <summary>
        ///     Sets up some of the local variables as well as Intrinsic and gesture detector
        /// </summary>
        private void InitializeElement()
        {
            SetScaleType(ScaleType.Matrix);
            _matrix = new Matrix();

            // If the current element is drawable
            if (Drawable != null)
            {
                _translationSize = new Size(Drawable.IntrinsicHeight, Drawable.IntrinsicWidth);
                SetOnTouchListener(this);
            }

            _gestureDetector = new GestureDetector(_context, new CustomImageViewGestureDetector(this));
            //SetAdjustViewBounds(true);
        }


        private float GetMatrixValue(int identifier)
        {
            var _values = new float[9];
            _matrix.GetValues(_values);
            return _values[identifier];
        }


        /// <summary>
        ///     Updates the matrix to correspond to zooming to a specific location on a defined scale
        /// </summary>
        public void ZoomTo(int x, int y, float scale)
        {
            // If we have a min scale defined and we would go under
            var scaling = Scale*scale;

            if (scaling < _minScale)
                scale = _minScale/Scale;
            else if (scale > 1 && scaling > _maxScale) // Same as above but for max scale
                scale = _maxScale/Scale;

            // Scale the same for width and height
            _matrix.PostScale(scale, scale);
            // Calculate post-scaling center values
            var scaleTranslateX = -(_elementSize.Width*scale - _elementSize.Width)/2;
            var scaleTranslateY = -(_elementSize.Height*scale - _elementSize.Height)/2;
            _matrix.PostTranslate(scaleTranslateX, scaleTranslateY);

            // Move the specified x and Y distance
            _matrix.PostTranslate(-(x - _elementSize.Width/2)*scale, 0);
            _matrix.PostTranslate(0, -(y - _elementSize.Height/2)*scale);

            ImageMatrix = _matrix;
        }

        public void PostTransitionCutting()
        {
            var width = (int) (_translationSize.Width*Scale);
            var height = (int) (_translationSize.Height*Scale);
            if (TranslateX < -(width - _elementSize.Width))
            {
                _matrix.PostTranslate(-(TranslateX + width - _elementSize.Width), 0);
            }

            if (TranslateX > 0)
            {
                _matrix.PostTranslate(-TranslateX, 0);
            }

            if (TranslateY < -(height - _elementSize.Height))
            {
                _matrix.PostTranslate(0, -(TranslateY + height - _elementSize.Height));
            }

            if (TranslateY > 0)
            {
                _matrix.PostTranslate(0, -TranslateY);
            }

            if (width < _elementSize.Width)
            {
                _matrix.PostTranslate((_elementSize.Width - width)/2, 0);
            }

            if (height < _elementSize.Height)
            {
                _matrix.PostTranslate(0, (_elementSize.Height - height)/2);
            }

            ImageMatrix = _matrix;
        }


        public override void SetImageBitmap(Bitmap bm)
        {
            base.SetImageBitmap(bm);
            InitializeElement();
        }

        public override void SetImageResource(int resId)
        {
            base.SetImageResource(resId);
            InitializeElement();
        }

        protected override bool SetFrame(int l, int t, int r, int b)
        {
            _elementSize = new Size(r - l, b - t);
            _matrix.Reset();
            var r_norm = r - l;
            _scale = r_norm/(float) _translationSize.Width;

            var paddingHeight = 0;
            var paddingWidth = 0;
            if (_scale*_translationSize.Height > _elementSize.Height)
            {
                _scale = _translationSize.Width/(float) _translationSize.Height;
                _matrix.PostScale(_scale, _scale);
                paddingWidth = (r - _elementSize.Width)/2;
            }
            else
            {
                _matrix.PostScale(_scale, _scale);
                paddingHeight = (b - _elementSize.Height)/2;
            }

            _matrix.PostTranslate(paddingWidth, paddingHeight);
            ImageMatrix = _matrix;
            _minScale = _scale;
            ZoomTo(_elementSize.Width/2, _elementSize.Height/2, _scale);
            PostTransitionCutting();
            return base.SetFrame(l, t, r, b);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (_gestureDetector.OnTouchEvent(e))
            {
                _lastMove = new Vector2(e.GetX(), e.GetY());
                return true;
            }

            var touchCount = e.PointerCount;
            switch (e.Action)
            {
                case MotionEventActions.Down:
                case MotionEventActions.Pointer1Down:
                case MotionEventActions.Pointer2Down:
                {
                    if (touchCount >= 2)
                    {
                        var touchOne = new Vector2(e.GetX(0), e.GetY(0));
                        var touchTwo = new Vector2(e.GetX(1), e.GetY(1));
                        var distance = touchOne.Distance2D(touchTwo);
                        _lastDistance = distance;
                        _isScaling = true;
                    }
                }
                    break;

                case MotionEventActions.Move:
                {
                    if (touchCount >= 2 && _isScaling)
                    {
                        var touchOne = new Vector2(e.GetX(0), e.GetY(0));
                        var touchTwo = new Vector2(e.GetX(1), e.GetY(1));
                        var distance = touchOne.Distance2D(touchTwo);
                        var scale = (distance - _lastDistance)/TotalElementDistance;
                        _lastDistance = distance;
                        scale += 1;
                        scale = scale*scale;
                        ZoomTo(_elementSize.Width/2, _elementSize.Height/2, scale);
                        PostTransitionCutting();
                    }
                    else if (!_isScaling)
                    {
                        var distanceX = _lastMove.X - e.GetX();
                        var distanceY = _lastMove.Y - (int) e.GetY();
                        _lastMove = new Vector2(e.GetX(), e.GetY());

                        _matrix.PostTranslate(-distanceX, -distanceY);
                        PostTransitionCutting();
                    }
                }
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Pointer1Up:
                case MotionEventActions.Pointer2Up:
                {
                    if (touchCount <= 1)
                    {
                        _isScaling = false;
                    }
                }
                    break;
            }
            return true;
        }

        public void MaxZoomTo(int x, int y)
        {
            if (_minScale != Scale && Scale - _minScale > 0.1f)
            {
                var scale = _minScale/Scale;
                ZoomTo(x, y, scale);
            }
            else
            {
                var scale = _maxScale/Scale;
                ZoomTo(x, y, scale);
            }
        }

        #region < Properties>

        // Event stuff
        public event LongPressHandler LongPress;

        // Element base
        private readonly Context _context;

        // Translation stuff
        private Matrix _matrix;
        private Size _elementSize;
        private Size _translationSize;
        private float _scale;
        private float _minScale;
        private readonly float _maxScale = 2.0f;
        private float _lastDistance;
        private Vector2 _lastMove;

        private bool _isScaling;
        private GestureDetector _gestureDetector;

        public float Scale
        {
            get { return GetMatrixValue(Matrix.MscaleX); }
        }

        public float TranslateX
        {
            get { return GetMatrixValue(Matrix.MtransX); }
        }

        public float TranslateY
        {
            get { return GetMatrixValue(Matrix.MtransY); }
        }

        public float TotalElementDistance
        {
            get { return (float) Math.Sqrt(Math.Pow(_elementSize.Width, 2) + Math.Pow(_elementSize.Height, 2)); }
        }

        public virtual void OnLongPress(MotionEvent e)
        {
            if (LongPress != null)
                LongPress(this, e);
        }

        #endregion

        #region < InheritedConstructors & Methods >

        public CustomImageView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            _context = context;
            InitializeElement();
        }

        public CustomImageView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            _context = context;
            InitializeElement();
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            return OnTouchEvent(e);
        }

        #endregion
    }
}