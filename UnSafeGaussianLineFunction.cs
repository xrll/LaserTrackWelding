using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CXLaser
{
    public static class GaussianLineFunction
    {
        public static double[] Integral4(double[] xdata,double[] zdata)
        {
            int n = xdata.Length;
            double a = 0, b = 0, c = 0, u = 0;
            double[,] M = new double[4, 4];
            double[,] V = new double[4, 1];
            double[] S = new double[n];
            double[] T = new double[n];
            double x1 = xdata[0];
            double y1 = zdata[0];
            for (int i = 0; i < n; i++)
            {
                double x = xdata[i];
                double y = zdata[i];
                double xx = x * x;
                double xx1 = x1 * x1;
                double dx1 = x - x1;
                double dy1 = y - y1;
                double dxx1 = xx - xx1;
                double xy = x * y;
                if (i > 0)
                {
                    double lx = xdata[i - 1];
                    double ly = zdata[i - 1];
                    double dx = (x - lx);
                    S[i] = S[i - 1] + 0.5 * (y + ly) * dx;
                    T[i] = T[i - 1] + 0.5 * (x * y + lx * ly) * dx;
                    V[0, 0] += S[i] * dy1;
                    V[1, 0] += T[i] * dy1;
                    V[2, 0] += dxx1 * dy1;
                    V[3, 0] += dx1 * dy1;
                }
                M[0, 0] += S[i] * S[i];
                M[0, 1] += S[i] * T[i];
                M[0, 2] += S[i] * dxx1;
                M[0, 3] += S[i] * dx1;
                M[1, 1] += T[i] * T[i];
                M[1, 2] += T[i] * dxx1;
                M[1, 3] += T[i] * dx1;
                M[2, 2] += dxx1 * dxx1;
                M[2, 3] += dxx1 * dx1;
                M[3, 3] += dx1 * dx1;
            }
            M[1, 0] = M[0, 1];
            M[2, 0] = M[0, 2];
            M[3, 0] = M[0, 3];
            M[2, 1] = M[1, 2];
            M[3, 1] = M[1, 3];
            M[3, 2] = M[2, 3];

            double[] pTmp = new double[4];
            for (int k = 0; k < 4; k++)
            {
                double w = M[0, 0];
                if (w == 0)
                    return null;
                int m = 4 - k - 1;
                for (int i = 1; i < 4; i++)
                {
                    double g = M[i, 0];
                    pTmp[i] = g / w;
                    if (i <= m)
                        pTmp[i] = -pTmp[i];
                    for (int j = 1; j <= i; j++)
                        M[(i - 1), j - 1] = M[i, j] + g * pTmp[j];
                }

                M[3, 3] = 1.0 / w;
                for (int i = 1; i < 4; i++)
                    M[3, i - 1] = pTmp[i];
            }
            for (int i = 0; i < 3; i++)
                for (int j = i + 1; j < 4; j++)
                    M[i, j] = M[j, i];

            double MV0 = M[0, 0] * V[0, 0] + M[0, 1] * V[1, 0] + M[0, 2] * V[2, 0] + M[0, 3] * V[3, 0];
            double MV1 = M[1, 0] * V[0, 0] + M[1, 1] * V[1, 0] + M[1, 2] * V[2, 0] + M[1, 3] * V[3, 0];
            u = -MV0 / MV1;
            c = -1 / MV1;
            double[] tk = new double[n];
            double[,] M2 = new double[2, 2];
            double[,] V2 = new double[2, 1];
            for (int i = 0; i < n; i++)
            {
                double g = Math.Exp(-0.5 * Math.Pow(Math.Abs(xdata[i] - u), 2) / c);
                M2[0, 0] += 1;
                M2[0, 1] += g;
                M2[1, 0] += g;
                M2[1, 1] += g * g;
                V2[0, 0] += zdata[i];
                V2[1, 0] += g * zdata[i];
            }
            double detM2 = M2[0, 0] * M2[1, 1] - M2[0, 1] * M2[1, 0];
            a = (M2[1, 1] * V2[0, 0] - M2[0, 1] * V2[1, 0]) / detM2;
            b = (-M2[1, 0] * V2[0, 0] + M2[0, 0] * V2[1, 0]) / detM2;
            return new double[] { a, b, c, u };
        }
        public static double[] Gradient_Descent(double[,] data, double[] a, double learningRate)
        {
            double alpha = 1.0;
            double minDeltaChi2 = 1e-30, threshold = 1e-6;
            a = new double[] { 1, 1, 1, 13, 1 };
            int n = data.GetLength(0);
            int num_iterations = 500000;
            int num = 20;
            int iter = 0;
            double[] na = new double[5];
            while (iter < num_iterations)
            {
                double[] jacobi = new double[5];
                double[] beta = new double[5];
                double chi2 = 0, incrementedChi2 = 0, dy = 0;
                int[] stochast = rnd(n, num);
                foreach (int i in stochast)
                {
                    dy = data[i, 1] - GenGaussian2(a[0], a[1], a[2], a[3], a[4], data[i, 0]);
                    jacobi[0] = 1;
                    double tmp = a[1] * Math.Exp(-0.5 * Math.Pow((data[i, 0] - a[3]), 2) / a[2]);
                    jacobi[1] = tmp / a[1];
                    jacobi[2] = 0.5 * Math.Pow((data[i, 0] - a[3]), 2) / (a[2] * a[2]) * tmp;
                    jacobi[3] = ((data[i, 0] - a[3])) / (a[2]) * tmp;
                    jacobi[4] = data[i, 0];

                    beta[0] += (2.0 / num) * dy * jacobi[0];
                    beta[1] += (2.0 / num) * dy * jacobi[1];
                    beta[2] += (2.0 / num) * dy * jacobi[2];
                    beta[3] += (2.0 / num) * dy * jacobi[3];
                    beta[4] += (2.0 / num) * dy * jacobi[4];
                }
                na[0] = (alpha * a[0] + learningRate * beta[0]);
                na[1] = (alpha * a[1] + learningRate * beta[1]);
                na[2] = (alpha * a[2] + learningRate * beta[2]);
                na[3] = (alpha * a[3] + learningRate * beta[3]);
                na[4] = (alpha * a[4] + learningRate * beta[4]);

                for (int i = 0; i < n; i++)
                {
                    dy = data[i, 1] - GenGaussian2(a[0], a[1], a[2], a[3], a[4], data[i, 0]);
                    chi2 += dy * dy;
                    dy = data[i, 1] - GenGaussian2(na[0], na[1], na[2], na[3], na[4], data[i, 0]);
                    incrementedChi2 += dy * dy;
                }
                if ((Math.Abs(chi2 - incrementedChi2) < minDeltaChi2 && chi2 < threshold))
                    return a;
                a = na;
                iter++;
            }
            return a;
        }
        static int[] rnd(int max, int num)
        {
            int[] a = new int[40];
            Random r = new Random();
            bool b;
            List<int> rev = new List<int>();
            int cindex = 0;
            while (cindex < num)
            {
                int t = r.Next(max);
                if (rev.Contains(t))
                    continue;
                rev.Add(t);
                cindex++;
            }
            return rev.ToArray();
        }
        public static double GenGaussian2(double a, double b, double c, double u, double d, double x)
        {
            //    y = a + b·exp(-½ (x - u)² / c)+ d * x
            return a + b * Math.Exp(-0.5 * Math.Pow(Math.Abs(x - u), 2) / c) + d * x;
        }
        public static  double gap = 2.2;
        public static double GenGaussianWidth(double b, double c)
        {
            return Math.Sqrt(-2 * c * Math.Log(gap / b));
        }

        public static unsafe double[] LevenbergMarquardt(double[,] data, double[] a, double lambda, out double err)
        {

            double minDeltaChi2 = 1e-30;
            //            double[] a = new double[] { 1, 1, 1, 13, 1 };
            int n = data.GetLength(0);
            err = 0;
            double[] da = new double[5];
            double[] na = new double[5];
            double[,] tj = new double[5, 5];
            double[] jacobi = new double[5];
            double[] beta = new double[5];
            for (int iter = 0; iter < 200; iter++)
            {
                da = new double[5];
                na = new double[5];
                tj = new double[5, 5];
                jacobi = new double[5];
                beta = new double[5];
                err = 0;
                fixed (double* d = data, ptj = tj, pj = jacobi, pb = beta)
                {
                    double* pd = d;
                    for (int i = 0; i < n; i++)
                    {
                        pj[0] = 1;
                        double tmp = a[1] * Math.Exp(-0.5 * Math.Pow((*pd - a[3]), 2) / a[2]);
                        pj[1] = tmp / a[1];
                        pj[2] = 0.5 * Math.Pow((*pd - a[3]), 2) / (a[2] * a[2]) * tmp;
                        pj[3] = (*pd - a[3]) / (a[2]) * tmp;
                        pj[4] = *pd;

                        ptj[0] += pj[0];
                        ptj[1] += pj[1];
                        ptj[2] += pj[2];
                        ptj[3] += pj[3];
                        ptj[4] += pj[4];

                        ptj[6] += pj[1] * pj[1];
                        ptj[7] += pj[1] * pj[2];
                        ptj[8] += pj[1] * pj[3];
                        ptj[9] += pj[1] * pj[4];

                        ptj[12] += pj[2] * pj[2];
                        ptj[13] += pj[2] * pj[3];
                        ptj[14] += pj[2] * pj[4];

                        ptj[18] += pj[3] * pj[3];
                        ptj[19] += pj[3] * pj[4];

                        ptj[24] += pj[4] * pj[4];

                        double dy = *(pd + 1) - GenGaussian2(a[0], a[1], a[2], a[3], a[4], *pd);
                        err += dy * dy;
                        pb[0] += dy * pj[0];
                        pb[1] += dy * pj[1];
                        pb[2] += dy * pj[2];
                        pb[3] += dy * pj[3];
                        pb[4] += dy * pj[4];
                        pd += 2;
                    }
                    ptj[0] *= (1.0 + lambda);
                    ptj[6] *= (1.0 + lambda);
                    ptj[12] *= (1.0 + lambda);
                    ptj[18] *= (1.0 + lambda);
                    ptj[24] *= (1.0 + lambda);


                    double* ca = ptj;
                    double G11 = Math.Sqrt(ca[0]);
                    double G12 = ca[1] / G11;
                    double G13 = ca[2] / G11;
                    double G14 = ca[3] / G11;
                    double G15 = ca[4] / G11;
                    ca += 5;
                    double G22 = Math.Sqrt(ca[1] - G12 * G12);
                    double G23 = (ca[2] - G12 * G13) / G22;
                    double G24 = (ca[3] - G12 * G14) / G22;
                    double G25 = (ca[4] - G12 * G15) / G22;
                    ca += 5;
                    double G33 = Math.Sqrt(ca[2] - G13 * G13 - G23 * G23);
                    double G34 = (ca[3] - G13 * G14 - G23 * G24) / G33;
                    double G35 = (ca[4] - G13 * G15 - G23 * G25) / G33;
                    ca += 5;
                    double G44 = Math.Sqrt(ca[3] - G14 * G14 - G24 * G24 - G34 * G34);
                    double G45 = (ca[4] - G14 * G15 - G24 * G25 - G34 * G35) / G44;
                    ca += 5;
                    double G55 = Math.Sqrt(ca[4] - G15 * G15 - G25 * G25 - G35 * G35 - G45 * G45);

                    pb[0] = pb[0] / G11;
                    pb[1] = (pb[1] - G12 * pb[0]) / G22;
                    pb[2] = (pb[2] - G13 * pb[0] - G23 * pb[1]) / G33;
                    pb[3] = (pb[3] - G14 * pb[0] - G24 * pb[1] - G34 * pb[2]) / G44;
                    pb[4] = (pb[4] - G15 * pb[0] - G25 * pb[1] - G35 * pb[2] - G45 * pb[3]) / G55;

                    pb[4] = pb[4] / G55;
                    pb[3] = (pb[3] - G45 * pb[4]) / G44;
                    pb[2] = (pb[2] - G34 * pb[3] - G35 * pb[4]) / G33;
                    pb[1] = (pb[1] - G23 * pb[2] - G24 * pb[3] - G25 * pb[4]) / G22;
                    pb[0] = (pb[0] - G12 * pb[1] - G13 * pb[2] - G14 * pb[3] - G15 * pb[4]) / G11;
                    for(int i=0;i<5;i++)
                    {
                        pb[i] += a[i];
                    }
                }
                na = beta;
                double incrementedChi2 = 0;
                for (int i = 0; i < n; i++)
                {
                    double dy = data[i, 1] - GenGaussian2(na[0], na[1], na[2], na[3], na[4], data[i, 0]);
                    incrementedChi2 += dy * dy;
                }
                if (Math.Abs(err - incrementedChi2) < minDeltaChi2)
                    return a;
                if (incrementedChi2 >= err)
                {
                    lambda *= 10;
                }
                else
                {
                    lambda /= 10;
                    a = na;
                }
            }
            return a;
        }
        public static double[] LevenbergMarquardt4(double[,] data, double lambda)
        {
            double minDeltaChi2 = 1e-30;
            double[] a = new double[] { 1, 1, 1, 16 };
            int n = data.GetLength(0);
            for (int iter = 0; iter < 300; iter++)
            {
                double[] da = new double[4];
                double[] na = new double[4];
                double[,] tj = new double[4, 4];
                double[] jacobi = new double[4];
                double[] beta = new double[4];
                double chi2 = 0;
                for (int i = 0; i < n; i++)
                {
                    jacobi[0] = 1;
                    double tmp = a[1] * Math.Exp(-0.5 * Math.Pow((data[i, 0] - a[3]), 2) / a[2]);
                    jacobi[1] = tmp / a[1];
                    jacobi[2] = 0.5 * Math.Pow((data[i, 0] - a[3]), 2) / (a[2] * a[2]) * tmp;
                    jacobi[3] = ((data[i, 0] - a[3])) / (a[2]) * tmp;

                    tj[0, 0] += jacobi[0];
                    tj[0, 1] += jacobi[1];
                    tj[0, 2] += jacobi[2];
                    tj[0, 3] += jacobi[3];

                    tj[1, 1] += jacobi[1] * jacobi[1];
                    tj[1, 2] += jacobi[1] * jacobi[2];
                    tj[1, 3] += jacobi[1] * jacobi[3];

                    tj[2, 2] += jacobi[2] * jacobi[2];
                    tj[2, 3] += jacobi[2] * jacobi[3];

                    tj[3, 3] += jacobi[3] * jacobi[3];

                    double dy = data[i, 1] - GenGaussian2(a[0], a[1], a[2], a[3], 0, data[i, 0]);
                    chi2 += dy * dy;
                    beta[0] += dy * jacobi[0];
                    beta[1] += dy * jacobi[1];
                    beta[2] += dy * jacobi[2];
                    beta[3] += dy * jacobi[3];
                }
                tj[0, 0] *= (1.0 + lambda);
                tj[1, 1] *= (1.0 + lambda);
                tj[2, 2] *= (1.0 + lambda);
                tj[3, 3] *= (1.0 + lambda);
                tj[1, 0] = tj[0, 1];
                tj[2, 0] = tj[0, 2];
                tj[2, 1] = tj[1, 2];
                tj[3, 0] = tj[0, 3];
                tj[3, 1] = tj[1, 3];
                tj[3, 2] = tj[2, 3];
                //    if (!PolynomialRegression.SymmetricMatrixInvert(tj))
                if (!InvertSsgj(ref tj))
                    continue;
                double[,] itj = tj;
                da[0] = (itj[0, 0] * beta[0] + itj[0, 1] * beta[1] + itj[0, 2] * beta[2] + itj[0, 3] * beta[3]);
                da[1] = (itj[1, 0] * beta[0] + itj[1, 1] * beta[1] + itj[1, 2] * beta[2] + itj[1, 3] * beta[3]);
                da[2] = (itj[2, 0] * beta[0] + itj[2, 1] * beta[1] + itj[2, 2] * beta[2] + itj[2, 3] * beta[3]);
                da[3] = (itj[3, 0] * beta[0] + itj[3, 1] * beta[1] + itj[3, 2] * beta[2] + itj[3, 3] * beta[3]);

                na[0] = (a[0] + da[0]);
                na[1] = (a[1] + da[1]);
                na[2] = (a[2] + da[2]);
                na[3] = (a[3] + da[3]);

                double incrementedChi2 = 0;
                for (int i = 0; i < n; i++)
                {
                    double dy = data[i, 1] - GenGaussian2(na[0], na[1], na[2], na[3], 0, data[i, 0]);
                    incrementedChi2 += dy * dy;
                }
                if (Math.Abs(chi2 - incrementedChi2) < minDeltaChi2)
                    return a;
                if (incrementedChi2 >= chi2)
                {
                    lambda *= 10;
                }
                else
                {
                    lambda /= 10;
                    a = na;
                }
            }
            return a;
        }
        public static bool InvertSsgj(ref double[,] Mat)
        {
            int i, j, k, m;
            double w, g;
            int numColumns = Mat.GetUpperBound(0) + 1;
            // 临时内存
            double[] pTmp = new double[numColumns];

            // 逐列处理
            for (k = 0; k <= numColumns - 1; k++)
            {
                w = Mat[0, 0];
                if (w == 0.0)
                {
                    return false;
                }

                m = numColumns - k - 1;
                for (i = 1; i <= numColumns - 1; i++)
                {
                    g = Mat[i, 0];
                    pTmp[i] = g / w;
                    if (i <= m)
                        pTmp[i] = -pTmp[i];
                    for (j = 1; j <= i; j++)
                        Mat[(i - 1), j - 1] = Mat[i, j] + g * pTmp[j];
                }

                Mat[numColumns - 1, numColumns - 1] = 1.0 / w;
                for (i = 1; i <= numColumns - 1; i++)
                    Mat[(numColumns - 1), i - 1] = pTmp[i];
            }

            // 行列调整
            for (i = 0; i <= numColumns - 2; i++)
                for (j = i + 1; j <= numColumns - 1; j++)
                    Mat[i, j] = Mat[j, i];

            return true;
        }
        public unsafe static double[] Integral(double[] xdata, double[] zdata)
        {
            int n = xdata.Length;
            double c = 0, u = 0;
            double[,] M = new double[5, 5];
            double[] V = new double[5];
            double S = 0, T = 0;
            fixed (double* px = xdata, pz = zdata, m = M, v = V)
            {
                double* cpx = px;
                double* cpz = pz;
                double x1 = *px;
                double y1 = *pz;
                for (int i = 0; i < n; i++)
                {
                    double x = *cpx;
                    double y = *cpz;
                    double dx1 = x - x1;
                    double dy1 = y - y1;
                    double dxx1 = x * x - x1 * x1;
                    double dxxx1 = (x * x * x - x1 * x1 * x1);
                    double xy = x * y;
                    if (i > 0)
                    {
                        double lx = cpx[-1];
                        double ly = cpz[-1];
                        double dx = (x - lx);
                        S += 0.5 * (y + ly) * dx;
                        T += 0.5 * (x * y + lx * ly) * dx;
                        v[0] += S * dy1;
                        v[1] += T * dy1;
                        v[2] += dxx1 * dy1;
                        v[3] += dx1 * dy1;
                        v[4] += dxxx1 * dy1;
                    }
                    m[0] += S * S;
                    m[1] += S * T;
                    m[2] += S * dxx1;
                    m[3] += S * dx1;
                    m[4] += S * dxxx1;

                    m[6] += T * T;
                    m[7] += T * dxx1;
                    m[8] += T * dx1;
                    m[9] += T * dxxx1;

                    m[12] += dxx1 * dxx1;
                    m[13] += dxx1 * dx1;
                    m[14] += dxx1 * dxxx1;

                    m[18] += dx1 * dx1;
                    m[19] += dx1 * dxxx1;

                    m[24] += dxxx1 * dxxx1;
                    cpx++;
                    cpz++;
                }
            }

            fixed (double* m = M, v = V)
            {
                double* ca = m;
                double G11 = Math.Sqrt(ca[0]);
                double G12 = ca[1] / G11;
                double G13 = ca[2] / G11;
                double G14 = ca[3] / G11;
                double G15 = ca[4] / G11;
                ca += 5;
                double G22 = Math.Sqrt(ca[1] - G12 * G12);
                double G23 = (ca[2] - G12 * G13) / G22;
                double G24 = (ca[3] - G12 * G14) / G22;
                double G25 = (ca[4] - G12 * G15) / G22;
                ca += 5;
                double G33 = Math.Sqrt(ca[2] - G13 * G13 - G23 * G23);
                double G34 = (ca[3] - G13 * G14 - G23 * G24) / G33;
                double G35 = (ca[4] - G13 * G15 - G23 * G25) / G33;
                ca += 5;
                double G44 = Math.Sqrt(ca[3] - G14 * G14 - G24 * G24 - G34 * G34);
                double G45 = (ca[4] - G14 * G15 - G24 * G25 - G34 * G35) / G44;
                ca += 5;
                double G55 = Math.Sqrt(ca[4] - G15 * G15 - G25 * G25 - G35 * G35 - G45 * G45);

                v[0] = v[0] / G11;
                v[1] = (v[1] - G12 * v[0]) / G22;
                v[2] = (v[2] - G13 * v[0] - G23 * v[1]) / G33;
                v[3] = (v[3] - G14 * v[0] - G24 * v[1] - G34 * v[2]) / G44;
                v[4] = (v[4] - G15 * v[0] - G25 * v[1] - G35 * v[2] - G45 * v[3]) / G55;

                v[4] = v[4] / G55;
                v[3] = (v[3] - G45 * v[4]) / G44;
                v[2] = (v[2] - G34 * v[3] - G35 * v[4]) / G33;
                v[1] = (v[1] - G23 * v[2] - G24 * v[3] - G25 * v[4]) / G22;
                v[0] = (v[0] - G12 * v[1] - G13 * v[2] - G14 * v[3] - G15 * v[4]) / G11;
                u = -v[0] / v[1];
                c = -1.0 / v[1];
            }


            double[,] M2 = new double[3, 3];
            double[] V2 = new double[3];
            fixed (double* px = xdata, pz = zdata, m2 = M2, v2 = V2)
            {
                double* cpx = px;
                double* cpz = pz;
                double x1 = *px;
                double y1 = *pz;
                for (int i = 0; i < n; i++)
                {
                    double g = Math.Exp(-0.5 * Math.Pow(Math.Abs(*cpx - u), 2) / c);
                    m2[0] += 1;
                    m2[1] += g;
                    m2[2] += *cpx;

                    m2[4] += g * g;
                    m2[5] += g * *cpx;

                    m2[8] += *cpx * *cpx;

                    v2[0] += *cpz;
                    v2[1] += g * *cpz;
                    v2[2] += *cpz * *cpx;
                    cpx++;
                    cpz++;
                }
                double lambda = 0.01;
                double A11 = M2[0, 0] + lambda;
                double A22 = M2[1, 1] + lambda;
                double A33 = M2[2, 2] + lambda;

                double G11 = Math.Sqrt(A11);
                double G12 = M2[0, 1] / G11;
                double G13 = M2[0, 2] / G11;

                double G22 = Math.Sqrt(A22 - G12 * G12);
                double G23 = (M2[1, 2] - G12 * G13) / G22;

                double G33 = Math.Sqrt(A33 - G13 * G13 - G23 * G23);

                v2[0] = v2[0] / G11;
                v2[1] = (v2[1] - G12 * v2[0]) / G22;
                v2[2] = (v2[2] - G13 * v2[0] - G23 * v2[1]) / G33;

                v2[2] = v2[2] / G33;
                v2[1] = (v2[1] - G23 * v2[2]) / G22;
                v2[0] = (v2[0] - G12 * v2[1] - G13 * v2[2]) / G11;
            }
            return new double[] { V2[0], V2[1], c, u, V2[2] };
        }
        public unsafe static double[] Integral(double[,] data)
        {
            int n = data.GetLength(0);
            double c = 0, u = 0;
            double[,] M = new double[5, 5];
            double[] V = new double[5];
            double S = 0, T = 0;
            fixed (double* d = data,m = M,v = V)
            {
                double* cd = d;
                double x1 = *d;
                double y1 = *(d+1);
                for (int i = 0; i < n; i++)
                {
                    double x = *cd;
                    double y = *(cd+1);
                    double dx1 = x - x1;
                    double dy1 = y - y1;
                    double dxx1 = x * x - x1 * x1;
                    double dxxx1 = (x * x * x - x1 * x1 * x1);
                    double xy = x * y;
                    if (i > 0)
                    {
                        double lx = cd[-2];
                        double ly = cd[-1];
                        double dx = (x - lx);
                        S += 0.5 * (y + ly) * dx;
                        T += 0.5 * (x * y + lx * ly) * dx;
                        v[0] += S * dy1;
                        v[1] += T * dy1;
                        v[2] += dxx1 * dy1;
                        v[3] += dx1 * dy1;
                        v[4] += dxxx1 * dy1;
                    }
                    m[0] += S * S;
                    m[1] += S * T;
                    m[2] += S * dxx1;
                    m[3] += S * dx1;
                    m[4] += S * dxxx1;

                    m[6] += T * T;
                    m[7] += T * dxx1;
                    m[8] += T * dx1;
                    m[9] += T * dxxx1;

                    m[12] += dxx1 * dxx1;
                    m[13] += dxx1 * dx1;
                    m[14] += dxx1 * dxxx1;

                    m[18] += dx1 * dx1;
                    m[19] += dx1 * dxxx1;

                    m[24] += dxxx1 * dxxx1;
                    cd += 2;
                }
            }

            fixed (double* m = M,v = V)
            {
                double* ca = m;
                double G11 = Math.Sqrt(ca[0]);
                double G12 = ca[1] / G11;
                double G13 = ca[2] / G11;
                double G14 = ca[3] / G11;
                double G15 = ca[4] / G11;
                ca += 5;
                double G22 = Math.Sqrt(ca[1] - G12 * G12);
                double G23 = (ca[2] - G12 * G13) / G22;
                double G24 = (ca[3] - G12 * G14) / G22;
                double G25 = (ca[4] - G12 * G15) / G22;
                ca += 5;
                double G33 = Math.Sqrt(ca[2] - G13 * G13 - G23 * G23);
                double G34 = (ca[3] - G13 * G14 - G23 * G24) / G33;
                double G35 = (ca[4] - G13 * G15 - G23 * G25) / G33;
                ca += 5;
                double G44 = Math.Sqrt(ca[3] - G14 * G14 - G24 * G24 - G34 * G34);
                double G45 = (ca[4] - G14 * G15 - G24 * G25 - G34 * G35) / G44;
                ca += 5;
                double G55 = Math.Sqrt(ca[4] - G15 * G15 - G25 * G25 - G35 * G35 - G45 * G45);

                v[0] = v[0] / G11;
                v[1] = (v[1] - G12 * v[0]) / G22;
                v[2] = (v[2] - G13 * v[0] - G23 * v[1]) / G33;
                v[3] = (v[3] - G14 * v[0] - G24 * v[1] - G34 * v[2]) / G44;
                v[4] = (v[4] - G15 * v[0] - G25 * v[1] - G35 * v[2] - G45 * v[3]) / G55;

                v[4] = v[4] / G55;
                v[3] = (v[3] - G45 * v[4]) / G44;
                v[2] = (v[2] - G34 * v[3] - G35 * v[4]) / G33;
                v[1] = (v[1] - G23 * v[2] - G24 * v[3] - G25 * v[4]) / G22;
                v[0] = (v[0] - G12 * v[1] - G13 * v[2] - G14 * v[3] - G15 * v[4]) / G11;
                u = -v[0] / v[1];
                c = -1.0 / v[1];
            }


            double[,] M2 = new double[3, 3];
            double[] V2 = new double[3];
            fixed (double* d = data,m2=M2, v2 = V2)
            {
                double* cd = d;
                double x1 = *d;
                double y1 = *(d+1); 
                for (int i = 0; i < n; i++)
                {
                    double g = Math.Exp(-0.5 * Math.Pow(Math.Abs(*cd - u), 2) / c);
                    m2[0] += 1;
                    m2[1] += g;
                    m2[2] += *cd;

                    m2[4] += g * g;
                    m2[5] += g * *cd;

                    m2[8] += *cd * *cd;

                    v2[0] += *(cd+1);
                    v2[1] += g * *(cd + 1);
                    v2[2] += *(cd + 1) * *(cd + 1);
                    cd += 2;
                }
                double lambda = 0.01;
                double A11 = M2[0, 0] + lambda;
                double A22 = M2[1, 1] + lambda;
                double A33 = M2[2, 2] + lambda;

                double G11 = Math.Sqrt(A11);
                double G12 = M2[0, 1] / G11;
                double G13 = M2[0, 2] / G11;

                double G22 = Math.Sqrt(A22 - G12 * G12);
                double G23 = (M2[1, 2] - G12 * G13) / G22;

                double G33 = Math.Sqrt(A33 - G13 * G13 - G23 * G23);

                v2[0] = v2[0] / G11;
                v2[1] = (v2[1] - G12 * v2[0]) / G22;
                v2[2] = (v2[2] - G13 * v2[0] - G23 * v2[1]) / G33;

                v2[2] = v2[2] / G33;
                v2[1] = (v2[1] - G23 * v2[2]) / G22;
                v2[0] = (v2[0] - G12 * v2[1] - G13 * v2[2]) / G11;
            }
            return new double[] { V2[0], V2[1], c, u, V2[2] };
        }

    }
}
