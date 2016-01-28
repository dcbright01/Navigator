using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator
{
    public class StepDetector
    {

        public delegate void StepTakenHandler();
        public event StepTakenHandler stepTaken;

        public StepDetector()
        {
            //constructor
        }

        private void stepCheck()
        {
            //Step checking algo
            //When you normally increase the counter, call onStepCheck
        }

        public void reset()
        {
            //reset all local variables
        }

        public void passValue()
        {
            //pre-process values
            //call stepCheck
        }

        protected void onStepTaken()
        {
            stepTaken();
        }
    }
}
