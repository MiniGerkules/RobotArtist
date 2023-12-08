﻿using System;

namespace GeneralComponents {
    public static class MathFunctions {
        public static double Plus(double a, double b) => a + b;
        public static double Minus(double a, double b) => a - b;
        public static double Multiply(double a, double b) => a * b;
        public static double Divide(double a, double b) => a / b;
        public static double Square(double a) => a * a;
        public static double Sqrt(double a) => Math.Sqrt(a);
        public static double SqrtFromAbs(double a) => Math.Sqrt(Math.Abs(a));
        public static double UnaryMinus(double a) => -a;
        public static double Exp(double a) => Math.Exp(a);
    }
}
