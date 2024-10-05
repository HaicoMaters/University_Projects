using System;
namespace AI
{
    public class CustomRecursionParameters<T>
    {

        public T input;

        public int iterativeStep;
        public int depth = 0;

        public CustomRecursionParameters(T x)
        {
            input = x;
            iterativeStep = 0;
        }
        public CustomRecursionParameters(T x, int s)
        {
            input = x;
            iterativeStep = s;
        }
    }
}

