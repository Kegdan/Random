using System;
using System.Collections.Generic;

namespace Random
{
    class Program
    {
        static void Main(string[] args)
        {
            var powerOfTwo = new PowerOfTwoRandomizer();
            Test(powerOfTwo,100);
            var prime = new PrimeRandomizer();
            Test(prime, 100);
            var macLoren = new MacLorenRandomizer(powerOfTwo,prime);
            Test(macLoren, 100);
        }

        private static float XSquare(IRandomizer r)
        {  // Критерий кси квадрат
            // таблица для проверки на https://ru.wikipedia.org/wiki/%D0%9A%D0%B2%D0%B0%D0%BD%D1%82%D0%B8%D0%BB%D0%B8_%D1%80%D0%B0%D1%81%D0%BF%D1%80%D0%B5%D0%B4%D0%B5%D0%BB%D0%B5%D0%BD%D0%B8%D1%8F_%D1%85%D0%B8-%D0%BA%D0%B2%D0%B0%D0%B4%D1%80%D0%B0%D1%82
            r.Reset();
            var bufferSize = 200;  // размер буффера
            var nichesSize = 10;  // колличество ниш (оно же - степени свободы +1)
            var buffer = new List<int>(bufferSize);
            for (int i = 0; i < bufferSize; i++) buffer.Add(r.GetNext());  // заполняем буфер
            var niches = new int[nichesSize]; 
            for (int i = 0; i < bufferSize; i++)
            {
                var index = (int)(buffer[i] / (((float)r.GetSeed()) / nichesSize));  // вычисляем в какую нишу попадает рандомное число 
                niches[index]++;
                
            }
            var expected = ((float)bufferSize) / nichesSize;  //  распределение равномерное, ожидается что во все ниши попадет одинаковое колличество
            var output = 0f;
            foreach (var niche in niches)
            {
                output += (niche - expected) * (niche - expected) / expected;  // хитрая формула
            }
            return output;
        }

        private static float SerialCorrelation(IRandomizer r)
        {
            // сериальная корреляция - смотрите формулу у кнута 
            r.Reset();
            var bufferSize = 200;
            var buffer = new List<int>(bufferSize);
            for (int i = 0; i < bufferSize; i++) buffer.Add(r.GetNext()); // заполняем буфер
            var sumM = buffer[0]*buffer[bufferSize - 1];
            var sumOfSquare = buffer[bufferSize - 1]*buffer[bufferSize - 1];
            var sum = buffer[bufferSize - 1];
            // всякие промежуточные значения
            for (int i = 0; i < bufferSize - 1; i++)
            {
                sumM += buffer[i]*buffer[i + 1];
                sumOfSquare += buffer[i]*buffer[i];
                sum += buffer[i];
            }
            sum *= sum;

            var output = ((float)(bufferSize * sumM - sum)) / (bufferSize * sumOfSquare - sum);// ну и вычисляем 

            // значения от -1 до 1, чем ближе к 0 - тем меньше корреляция - тем лучше
            return output;
        }

        static void Test(IRandomizer r, int size)
        {// красивый тест
            Console.WriteLine("Test for {0}, size {1}",typeof(IRandomizer),size);
            for (int i = 0; i < size-1; i++)
                Console.Write("{0}, ", r.GetNext());

            Console.WriteLine(r.GetNext());

            Console.WriteLine();
            Console.WriteLine("XSquare = {0}", XSquare(r));
            Console.WriteLine();
            Console.WriteLine("Serial Correlation = {0}", SerialCorrelation(r));
            Console.WriteLine();
          
        }
    }



    interface IRandomizer // интерфейс для рандомизаторов
    {
       int GetNext(); 

       int GetSeed(); 

       void Reset();
    }

    class PowerOfTwoRandomizer : IRandomizer
    {
        private const int A = 4*7 + 1;
        private const int C = 37;
        private const int N = 256;
        // константы из головы
        private int _state = 1;

        public int GetNext()
        {
            _state = (_state*A + C) % N;
            return _state;
        }

        public int GetSeed()
        {
            return N;
        }

        public void Reset()
        {
            _state = 1;
        }
    }

    class PrimeRandomizer : IRandomizer
    {
        private const int A = 5;
        private const int N = 263;
        // константы из головы
        private int _state = 1;

        public int GetNext()
        {
            _state = (_state * A ) % N;
            return _state;
        }

        public int GetSeed()
        {
            return N;
        }

        public void Reset()
        {
            _state = 1;
        }
    }

    class MacLorenRandomizer : IRandomizer
    {
        private IRandomizer _first;
        private IRandomizer _second;
        private const int BufferSize = 100;
        private int[] _buffer = new int[BufferSize];
        public MacLorenRandomizer(IRandomizer f,IRandomizer s)
        {
            _first = f;
            _second = s;
            Initialize();
        }

        private void Initialize()
        {
            _first.Reset();
            _second.Reset();
            for (int i = 0; i < BufferSize; i++)
            {
                _buffer[i] = _first.GetNext();
            }
        }

        public int GetNext()
        {  // суть - заполняем буффер рандомными числами из одного рандома
            // берем рандом из другого и нормализируем что бы получился индекс эллемента из буффера
            var index = (int)(_second.GetNext()*(((float)BufferSize)/_second.GetSeed()));
            // то что лежит по индексу - следующее рандомное число
            var output = _buffer[index];
            // а на его место ложим новое
            _buffer[index] = _first.GetNext();
            return output;
        }

        public int GetSeed()
        {
          return _first.GetSeed();
        }

        public void Reset()
        {
            _first.Reset();
            _second.Reset();
            Initialize();
        }
    }
}
