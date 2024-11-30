using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Trajectory
{
    public class MinimumSnapTrajectory
    {
        private double _startPos;
        private double _startVel;
        private double _startAcc;
        private double _endPos;
        private double _endVel;
        private double _endAcc;
        private double _T;
        public double[] Coefficients;
        public MinimumSnapTrajectory(double startPos, double startVel, double startAcc, double endPos, double endVel, double endAcc, double T)
        {
            _startPos = startPos;
            _startVel = startVel;
            _startAcc = startAcc;
            _endPos = endPos;
            _endVel = endVel;
            _endAcc = endAcc;
            _T = T;
            Coefficients = MinimumSnapCoefficients();
        }
        private double[] MinimumSnapCoefficients()
        {
            var A = Matrix<double>.Build.DenseOfArray(new double[,]
            {
            {1, 0, 0,    0,    0,    0},
            {0, 1, 0,    0,    0,    0},
            {0, 0, 2,    0,    0,    0},
            {1, _T, Math.Pow(_T, 2), Math.Pow(_T, 3), Math.Pow(_T, 4), Math.Pow(_T, 5)},
            {0, 1, 2*_T,  3*Math.Pow(_T, 2), 4*Math.Pow(_T, 3), 5*Math.Pow(_T, 4)},
            {0, 0, 2,    6*_T,  12*Math.Pow(_T, 2), 20*Math.Pow(_T, 3)}
            });

            var b = Vector<double>.Build.Dense(new double[]
            {
            _startPos, _startVel, _startAcc, _endPos, _endVel, _endAcc
            });

            var x = A.Solve(b);

            return x.ToArray();
        }

        // Evaluate the polynomial at a given time t
        public double EvaluatePolynomial(double t)
        {
            double result = 0;
            for (int i = 0; i < Coefficients.Length; i++)
            {
                result += Coefficients[i] * Math.Pow(t, i);
            }
            return result;
        }

        // Evaluate the first derivative (velocity) of the polynomial at time t
        public double EvaluatePolynomialDerivative(double t)
        {
            double result = 0;
            for (int i = 1; i < Coefficients.Length; i++)  // Start at i=1 because the derivative of a0 is 0
            {
                result += i * Coefficients[i] * Math.Pow(t, i - 1);
            }
            return result;
        }

        // Evaluate the second derivative (acceleration) of the polynomial at time t
        public double EvaluatePolynomialSecondDerivative(double t)
        {
            double result = 0;
            for (int i = 2; i < Coefficients.Length; i++)  // Start at i=2 because the second derivative of a0 and a1 is 0
            {
                result += i * (i - 1) * Coefficients[i] * Math.Pow(t, i - 2);
            }
            return result;
        }
    }
}
