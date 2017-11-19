namespace MessageQueueTask.Actions
{
    class CalcAction : IAction
    {
        public int Proceed(int number)
        {
            return number * number;
        }
    }
}
