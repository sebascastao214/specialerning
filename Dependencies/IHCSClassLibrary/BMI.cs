using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace IHCSClassLibrary
{
    public class BMI
    {

        public double BmiPounds;
        public double BmiKilograms;

        public double BMI_PoundsInches(double WeightPounds, double HeightInches)
        {
            BmiPounds = Math.Round((WeightPounds * 703) / Math.Pow(HeightInches, 2), 2);

            return BmiPounds;
        }

        public double BMI_KilogramMeters(double WeightKG, double HeightMeters)
        {
            BmiKilograms = Math.Round((WeightKG) / Math.Pow(HeightMeters, 2) * 10000, 2);

            return BmiKilograms;
        }

        
        public String Result;

        public void BmiResults(double BmiValue)
        {
            if (BmiValue < 18.5)
            {

                Result = "Underweight";
            }
            else if (BmiValue > 18.50 & BmiValue < 24.9)
            {
                Result = "Normal";
                
            }
            else if (BmiValue > 25 & BmiValue < 29.9)
            {
                Result = "Overweight";
                
            }
            else if (BmiValue > 30)
            {
                Result = "Obese";
            }
            
        }

        public string GetBMIResult()
        {
            return Result;
        }
        
        
    }
}
