using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SumaSP
{
    class ButtonClass
    {
        public void prueba()
        {
            MessageBox.Show("ha funcionado");
        }

        int miSuma;
        //public int suma (int numero1,int numero2,int numero3, int numero4, int numero5, int numero6,
        //    int numero7, int numero8, int numero9)
        public int suma(int numero1, int numero2)
        {
            miSuma = numero1 + numero2;
            
            return miSuma;
        }

        int miResta;
        public int resta (int numero1, int numero2)
        {
            miResta = numero1 - numero2;

            return miResta;
        }

        int miMultiplicacion;
        public int multiplicacion (int numero1, int numero2)
        {
            miMultiplicacion = numero1 * numero2;

            return miMultiplicacion;
        }

        int miDivision;
        public double division (int numero1, int numero2)
        {
            miDivision = numero1 / numero2;

            return miDivision;
        }
    }
}
