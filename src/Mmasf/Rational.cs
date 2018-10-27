using System;
using hw.DebugFormatter;
using hw.Helper;


namespace ManageModsAndSavefiles
{
    sealed class Rational : DumpableObject, IAggregateable<Rational>
    {
        public readonly int Numerator;
        public readonly int Denominator;

        public Rational(int numerator, int denominator = 1)
        {
            if(denominator == 0)
            {
                Numerator = Math.Sign(numerator);
                Denominator = 0;
                return;
            }

            if(denominator < 0)
            {
                denominator = -denominator;
                numerator = -numerator;
            }

            var greatestCommonDenominator = GreatestCommonDenominator(denominator, Math.Abs(numerator));
            Numerator = numerator / greatestCommonDenominator;
            Denominator = denominator / greatestCommonDenominator;
        }

        public int Ceiling => Denominator == 1 ? Numerator : (int) Math.Ceiling(Numerator / (double) Denominator);

        protected override string GetNodeDump()
        {
            if(Denominator == 1)
                return Numerator.ToString();
            return Numerator + ":" + Denominator;
        }

        Rational IAggregateable<Rational>.Aggregate(Rational other) => this + other;
        public override string ToString() { return NodeDump; }

        public static bool operator ==(Rational a, Rational b)
        {
            return Equals(a, b)
                   || !Equals(a, null)
                   && !Equals(b, null)
                   && a.Denominator == b.Denominator
                   && a.Numerator == b.Numerator;
        }

        public static bool operator ==(int a, Rational b) =>
            1 == b.Denominator && a == b.Numerator;

        public static bool operator ==(Rational a, int b) =>
            a.Denominator == 1 && a.Numerator == b;

        public static bool operator !=(Rational a, Rational b) => !(a == b);
        public static bool operator !=(int a, Rational b) => !(a == b);
        public static bool operator !=(Rational a, int b) => !(a == b);

        public static bool operator <(Rational a, Rational b) =>
            a.Numerator * b.Denominator < b.Numerator * a.Denominator;


        public static bool operator >(Rational a, Rational b) => b < a;
        public static bool operator <(Rational a, int b) => a < new Rational(b);
        public static bool operator >(Rational a, int b) => a > new Rational(b);

        public static Rational operator /(Rational a, Rational b) =>
            new Rational(a.Numerator * b.Denominator, a.Denominator * b.Numerator);

        public static Rational operator /(Rational a, int b) =>
            new Rational(a.Numerator, a.Denominator * b);

        public static Rational operator /(int a, Rational b) =>
            new Rational(a * b.Denominator, b.Numerator);

        public static Rational operator *(Rational a, Rational b) =>
            new Rational(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

        public static Rational operator *(Rational a, int b) =>
            new Rational(a.Numerator * b, a.Denominator);

        public static Rational operator *(int a, Rational b) =>
            new Rational(a * b.Numerator, b.Denominator);

        public static Rational operator +(Rational a, Rational b)
            => new Rational(
                a.Numerator * b.Denominator + a.Denominator * b.Numerator,
                a.Denominator * b.Denominator
            );

        public static Rational operator +(Rational a, int b) =>
            new Rational(a.Numerator + a.Denominator * b, a.Denominator);

        public static Rational operator +(int b, Rational a) =>
            new Rational(a.Numerator + a.Denominator * b, a.Denominator);


        static int GreatestCommonDenominator(int a, int b)
        {
            while(a != 0 && b != 0)
            {
                if(a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a == 0 ? b : a;
        }
    }
}