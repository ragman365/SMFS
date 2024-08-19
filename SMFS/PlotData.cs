using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GeneralLib;
using ZedGraph;
using ZedGraph.Web;
/***********************************************************************************************/
namespace SMFS
{
/***********************************************************************************************/
    public partial class PlotData : Form
    {
        bool Loading = true;
        private DataGrid work_dg;
        private DataTable work_dt;
/***********************************************************************************************/
        struct LineStyle
        {
            public Color color;
            public SymbolType symbolType;
        }
        private LineStyle[] LineStyles;
        private int LineStylesIndex = 0;
        private double XMinPoint, XMaxPoint, YMinPoint, YMaxPoint;
/***********************************************************************************************/
        public PlotData( DataTable dt )
        {
            work_dt = dt;
            work_dg = null;
            InitializeComponent();
        }
/***********************************************************************************************/
        private void PlotData_Load(object sender, EventArgs e)
        {
            Loading = true;
            SetupDataBox();
            SetupTimeInterval();
            Loading = false;
        }
/***********************************************************************************************/
        private void SetupTimeInterval()
        {
            long min = 0L;
            long max = 0L;
            DataTable dt = work_dt;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string date = dt.Rows[i]["Date"].ToString();
                long ldate = G1.date_to_days(date);
                if (i == 0)
                    min = ldate;
                if (ldate < min)
                    min = ldate;
                if (ldate > max)
                    max = ldate;
            }
            string sdate = G1.days_to_date(min);
            DateTime dte = sdate.ObjToDateTime();
            this.dateTimePicker1.Value = dte;
            sdate = G1.days_to_date(max);
            dte = sdate.ObjToDateTime();
            this.dateTimePicker2.Value = dte;
        }
/***********************************************************************************************/
        private void SetupDataBox()
        {
            DataTable dt = work_dt;
            dataBox.Items.Clear();
            bool FoundDate = false;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string name = dt.Columns[i].ColumnName.ToString();
                if (name.Trim().ToUpper() == "DATE")
                    FoundDate = true;
                dataBox.Items.Add(name);
            }
            if (FoundDate)
                dataBox.Items.Add("MyCPI");
        }
/***********************************************************************************************/
        private void dataBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Loading)
                return;
            UpdatePlot(zedPlot);
        }
/***********************************************************************************************/
		private string build_title ()
		{
			string title = "Plot";
			return title;
		}
/***********************************************************************************************/
        private void AddCPIData(PointPairList[] dataPoints, int index, string date, double cpi)
        {
            double x = (double)G1.date_to_days(date);
            dataPoints[index].Add(x, cpi);
            if (x < XMinPoint)
                XMinPoint = x;
            else if (x > XMaxPoint)
                XMaxPoint = x;
            if (cpi < YMinPoint)
                YMinPoint = cpi;
            if (cpi > YMaxPoint)
                YMaxPoint = cpi;
        }
/***********************************************************************************************/
        private void LoadCPI(PointPairList[] dataPoints, int index)
        {
            AddCPIData(dataPoints, index, "09/30/1999", 167.9D);
            AddCPIData(dataPoints, index, "09/30/2000", 173.7D);
            AddCPIData(dataPoints, index, "09/30/2001", 178.3D);
            AddCPIData(dataPoints, index, "09/30/2002", 181.0D);
            AddCPIData(dataPoints, index, "09/30/2003", 185.2D);
            AddCPIData(dataPoints, index, "09/30/2004", 189.9D);
            AddCPIData(dataPoints, index, "09/30/2005", 198.8D);
            AddCPIData(dataPoints, index, "09/30/2006", 202.9D);
            AddCPIData(dataPoints, index, "09/30/2007", 208.49D);
            AddCPIData(dataPoints, index, "09/30/2008", 218.73D);
            AddCPIData(dataPoints, index, "09/30/2009", 215.96D);
            AddCPIData(dataPoints, index, "09/30/2010", 218.44D);
        }
/***********************************************************************************************/
        private void PopulateLineStyles()
        {
            if (LineStyles != null)
                return;
            LineStyles = new LineStyle[17];
            LineStyles[0].color = Color.DarkBlue;
            LineStyles[0].symbolType = SymbolType.Circle;
            LineStyles[1].color = Color.DarkCyan;
            LineStyles[1].symbolType = SymbolType.Diamond;
            LineStyles[2].color = Color.DarkGray;
            LineStyles[2].symbolType = SymbolType.HDash;
            LineStyles[3].color = Color.DarkGreen;
            LineStyles[3].symbolType = SymbolType.Plus;
            LineStyles[4].color = Color.DarkMagenta;
            LineStyles[4].symbolType = SymbolType.Square;
            LineStyles[5].color = Color.DarkOliveGreen;
            LineStyles[5].symbolType = SymbolType.Star;
            LineStyles[6].color = Color.DarkOrange;
            LineStyles[6].symbolType = SymbolType.Triangle;
            LineStyles[7].color = Color.DarkOrchid;
            LineStyles[7].symbolType = SymbolType.TriangleDown;
            LineStyles[8].color = Color.DarkRed;
            LineStyles[8].symbolType = SymbolType.VDash;
            LineStyles[9].color = Color.DarkSalmon;
            LineStyles[9].symbolType = SymbolType.XCross;
            LineStyles[10].color = Color.DarkSeaGreen;
            LineStyles[10].symbolType = SymbolType.Circle;
            LineStyles[11].color = Color.DarkSlateBlue;
            LineStyles[11].symbolType = SymbolType.Diamond;
            LineStyles[12].color = Color.DarkSlateGray;
            LineStyles[12].symbolType = SymbolType.HDash;
            LineStyles[13].color = Color.DarkTurquoise;
            LineStyles[13].symbolType = SymbolType.Plus;
            LineStyles[14].color = Color.DarkViolet;
            LineStyles[14].symbolType = SymbolType.Square;
            LineStyles[15].color = Color.DeepPink;
            LineStyles[15].symbolType = SymbolType.Star;
            LineStyles[16].color = Color.DeepSkyBlue;
            LineStyles[16].symbolType = SymbolType.Triangle;
        }
/***********************************************************************************************/
        private void UpdatePlot(ZedGraphControl zgc)
        {
            LineStylesIndex = 0;
            XMinPoint = int.MaxValue;
            XMaxPoint = int.MinValue;
            YMinPoint = int.MaxValue;
            YMaxPoint = int.MinValue;

            PopulateLineStyles();

            zgc.IsAntiAlias = true;
            zgc.IsAutoScrollRange = true;
            zgc.IsShowHScrollBar = true;
            zgc.IsShowVScrollBar = true;

            MasterPane masterPane = zgc.MasterPane;
            masterPane.PaneList.Clear();
            string title = build_title();
            masterPane.Title.Text = title;
            masterPane.Title.FontSpec = new FontSpec("Times New Roman", 10F, Color.Black, false, false, false );
            masterPane.Title.IsVisible = true;
            masterPane.Fill = new Fill(Color.White, Color.MediumSlateBlue, 45.0F);
            masterPane.Margin.All = 10;
            masterPane.InnerPaneGap = 10;
            masterPane.Legend.IsVisible = true;
            masterPane.Legend.Position = LegendPos.TopCenter;

            DataTable WorkingDataTable = work_dt;

            int numCurves = dataBox.CheckedItems.Count;
            PointPairList[] dataPoints = new PointPairList[numCurves];
            string[] pointPairReferenceName = new string[numCurves];
            int pointIndex = 0;
            string[] names = new string[dataBox.CheckedItems.Count];
            int count = 0;
            foreach (string name in dataBox.CheckedItems)
            {
                names[count] = name;
                count++;
            }

            DateTime dte = this.dateTimePicker1.Value;
            string date = dte.Month.ToString("D2") + "/" + dte.Day.ToString("D2") + "/" + dte.Year.ToString("D2");
            long min_date = G1.date_to_days(date);

            dte = this.dateTimePicker2.Value;
            date = dte.Month.ToString("D2") + "/" + dte.Day.ToString("D2") + "/" + dte.Year.ToString("D2");
            long max_date = G1.date_to_days(date);
            for ( int kk=0; kk<count; kk++ )
            {
                string name = names[kk];
                dataPoints[pointIndex] = new PointPairList();
                pointPairReferenceName[pointIndex] = name;
                bool containsData = false;
                if (name.Trim().ToUpper() == "MYCPI")
                {
                    LoadCPI(dataPoints, pointIndex);
                    containsData = true;
                }
                else
                {
                    //dataPoints[pointIndex].Add(1D, 100D);
                    //dataPoints[pointIndex].Add(2D, 50D);
                    //dataPoints[pointIndex].Add(3D, 33D);
                    //dataPoints[pointIndex].Add(4D, 25D);
                    //dataPoints[pointIndex].Add(5D, 20D);
                    //containsData = true;
                    //XMinPoint = 1D;
                    //XMaxPoint = 5D;
                    //YMinPoint = 20D;
                    //YMaxPoint = 100D;
                    foreach (DataRow dr in WorkingDataTable.Rows)
                    {
                        date = dr["Date"].ToString();
                        if (date.Trim().ToUpper() == "TOTALS")
                            break;
                        long ldate = G1.date_to_days(date);
                        if (ldate < min_date || ldate > max_date)
                            continue;
                        double myXMonths = ldate;
                        if (myXMonths < XMinPoint)
                            XMinPoint = myXMonths;
                        if (myXMonths > XMaxPoint)
                            XMaxPoint = myXMonths;
                        double myYvalue = dr[name].ObjToDouble();
                        if (!this.chkActual.Checked)
                        {
                            if (myYvalue < 0D)
                                myYvalue = myYvalue * -1D;
                        }
                        if (zeroBox.Checked && myYvalue == 0.0D)
                            continue;
                        if (myYvalue < YMinPoint)
                            YMinPoint = myYvalue;
                        if (myYvalue > YMaxPoint)
                            YMaxPoint = myYvalue;
                        containsData = true;
                        dataPoints[pointIndex].Add(myXMonths, myYvalue);
                    }
                }
                if (zeroBox.Checked && !containsData)
                    pointPairReferenceName[pointIndex] = "";
                pointIndex++;
            }




            if (!this.multiCheckBox.Checked)
            {
                GraphPane myPane = GetGraphPanel(dataPoints, pointPairReferenceName );
                double xmin = XMinPoint;
                double xmax = XMaxPoint;
                double spacing = 5D;
                adjust_scale(ref xmin, ref xmax, ref spacing);
                myPane.XAxis.Scale.MajorStep = spacing;
                myPane.XAxis.Scale.MinorStep = spacing / 2D;
                myPane.XAxis.Scale.Min = xmin;
                myPane.XAxis.Scale.Max = xmax;
                myPane.XAxis.Scale.FontSpec = new FontSpec("Times New Roman", 9F, Color.Black, false, false, false);
                myPane.XAxis.Title.Text = "Month/Year";

                double ymin = YMinPoint;
                double ymax = YMaxPoint;
                spacing = 5D;
                adjust_scale(ref ymin, ref ymax, ref spacing);
                myPane.YAxis.Scale.MajorStep = spacing;
                myPane.YAxis.Scale.MinorStep = spacing / 2D;
                myPane.YAxis.Scale.Min = ymin;
                myPane.YAxis.Scale.Max = ymax;

                masterPane.Add(myPane);
            }
            else
            {
                for (int index = 0; index < dataPoints.Length; index++)
                {
                    if (!String.IsNullOrEmpty(pointPairReferenceName[index]))
                    {
                        GraphPane myPane = (GetGraphPanel(new PointPairList[] { dataPoints[index] }, new string[] { pointPairReferenceName[index] }));
                        double xmin = XMinPoint;
                        double xmax = XMaxPoint;
                        double spacing = 5D;
                        get_minmax(true, dataPoints[index], ref xmin, ref xmax);
                        adjust_scale(ref xmin, ref xmax, ref spacing);
                        myPane.XAxis.Scale.MajorStep = spacing;
                        myPane.XAxis.Scale.MinorStep = spacing / 2D;
                        myPane.XAxis.Scale.Min = xmin;
                        myPane.XAxis.Scale.Max = xmax;
                        string name = pointPairReferenceName[index].ToString();
                        myPane.XAxis.Title.Text = name + " " + "Month/Year";

                        double ymin = YMinPoint;
                        double ymax = YMaxPoint;
                        spacing = 5D;
                        get_minmax(false, dataPoints[index], ref ymin, ref ymax);
                        adjust_scale(ref ymin, ref ymax, ref spacing);
                        myPane.YAxis.Scale.MajorStep = spacing;
                        myPane.YAxis.Scale.MinorStep = spacing / 2D;
                        myPane.YAxis.Scale.Min = ymin;
                        myPane.YAxis.Scale.Max = ymax;

                        masterPane.Add(myPane);

                    }
                }
            }
            using (Graphics g = CreateGraphics())
            {
                masterPane.SetLayout(g, PaneLayout.SquareColPreferred);
                zgc.AxisChange();
            }

            zgc.Refresh();
        }
/***********************************************************************************************/
        private GraphPane GetGraphPanel(PointPairList[] dataPoints, string[] titles )
        {
            GraphPane graphPane = new GraphPane();
            graphPane.Title.IsVisible = false;
            graphPane.XAxis.Title.Text = "Months";
            graphPane.YAxis.Title.Text = "Value Scale";
            graphPane.XAxis.MajorGrid.IsVisible = true;
            graphPane.YAxis.MajorGrid.IsVisible = true;
            graphPane.Legend.IsVisible = false;
            graphPane.Fill = new Fill(Color.White, Color.LightYellow, 45.0F);
            graphPane.BaseDimension = 6.0F;
            
            for (int index = 0; index < dataPoints.Length; index++)
                if (!String.IsNullOrEmpty(titles[index]))
                {
                    LineItem curve = graphPane.AddCurve(titles[index], dataPoints[index], LineStyles[LineStylesIndex].color, LineStyles[LineStylesIndex].symbolType);
                    if (checkBox_smoothLines.Checked)
                    {
                        curve.Line.IsSmooth = true;
                        curve.Line.SmoothTension = 0.3F;
                    }

                    if (checkBox_showTrendLines.Checked)
                    {
                        List<PointD> pointList = new List<PointD>();
                        for (int i = 0; i < dataPoints[index].Count; i++)
                        {
                            List<PointD> L = new List<PointD>();
                            PointD p = new PointD();
                            p.X = (double)(dataPoints[index][i].X);
                            p.Y = (double)(dataPoints[index][i].Y);
                            pointList.Add(p);
                        }

                        List<PointD> outList = new List<PointD>();
                        MyRegression(pointList, ref outList);

                        PointPairList trendData = new PointPairList();
                        for (int i = 0; i < outList.Count; i++)
                        {
                            double x = outList[i].X;
                            double y = outList[i].Y;
                            trendData.Add((double)x, (double)y);
                        }
                        LineItem trendLine = graphPane.AddCurve("", trendData, LineStyles[LineStylesIndex].color, SymbolType.None);
                        trendLine.Line.Width = 2;
                        trendLine.Label.IsVisible = false;
                    }

                    if (chkRegress.Checked )
                    {
                        List<PointD> pointList = new List<PointD>();
                        for (int i = 0; i < dataPoints[index].Count; i++)
                        {
                            List<PointD> L = new List<PointD>();
                            PointD p = new PointD();
                            p.X = (double)(dataPoints[index][i].X);
                            p.Y = (double)(dataPoints[index][i].Y);
                            pointList.Add(p);
                        }

                        string d = txtDegree.Text;
                        int degree = 2;
                        if (G1.validate_numeric(d))
                        {
                            degree = G1.myint(d);
                            if (degree <= 0 || degree > 10 )
                                degree = 3;
                        }

                        List<PointD> outList = new List<PointD>();
                        MyPoly ( degree, pointList, ref outList);

                        PointPairList trendData = new PointPairList();
                        for (int i = 0; i < outList.Count; i++)
                        {
                            double x = outList[i].X;
                            double y = outList[i].Y;
                            trendData.Add((double)x, (double)y);
                        }
                        LineItem trendLine = graphPane.AddCurve("", trendData, LineStyles[LineStylesIndex].color, SymbolType.None);
                        trendLine.Line.Width = 3;
                        trendLine.Label.IsVisible = false;
                    }
                    //if (checkBox_showTrendLines.Checked)
                    //{
                    //    PointPairList trendData = new PointPairList();

                    //    double total = 0;
                    //    foreach (PointPair pp in dataPoints[index])
                    //        total += pp.Y;

                    //    double AVG = dataPoints[index].Count > 0 ? (total / dataPoints[index].Count) : 0;

                    //    Double Slope = (dataPoints[index][0].Y - AVG) / (dataPoints[index][0].X - dataPoints[index][dataPoints[index].Count - 1].X);
                    //    Double Offset = AVG - (Slope * dataPoints[index][(int)(dataPoints[index].Count / 2)].X);

                    //    foreach (PointPair pp in dataPoints[index])
                    //        trendData.Add(pp.X, Offset + (Slope * pp.X));

                    //    LineItem trendLine = graphPane.AddCurve("", trendData, LineStyles[LineStylesIndex].color, SymbolType.None);
                    //    trendLine.Line.Width = 2;
                    //    trendLine.Label.IsVisible = false;
                    //}

                    LineStylesIndex++;
                }

            if (dateTimeBox.Checked)
                graphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            graphPane.YAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(YAxis_ScaleFormatEvent);


            return graphPane;
        }
/********************************************************************************************/
        string YAxis_ScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
        {
            double value = (double)val;
            string str = value.ToString();
            int left = str.IndexOf(".");
            int right = str.Length - left;
            string answer = str;
            if ( left > 0 )
                answer = str.Substring(0, left);
            return answer;
        }
/********************************************************************************************/
        private void get_minmax ( bool XDir, PointPairList dataPoints, ref double min, ref double max )
        {
            min = int.MaxValue;
            max = int.MinValue;
            for (int i = 0; i < dataPoints.Count; i++)
            {
                double myValue = dataPoints[i].X;
                if (!XDir)
                    myValue = dataPoints[i].Y;
                if (myValue < min)
                    min = myValue;
                if (myValue > max)
                    max = myValue;
            }
        }
/********************************************************************************************/
        private void adjust_scale(ref double min, ref double max, ref double spacing)
        {
            double incr   = 1D;
            double minout = min;
            double maxout = max;
            double total  = max - min;
            if (total == 0D)
                return;
            int num_units        = 3;
            double[] scale_units = new double[5];
            int num_divs         = 3;
            double[] scale_div   = new double[5];
            scale_units[0]       = 1D;
            scale_units[1]       = 2D;
            scale_units[2]       = 5D;
            scale_div[0]         = 6D;
            scale_div[1]         = 8D;
            scale_div[2]         = 10D;
            int first            = -2;
            for (int i = first; i <= 6; i++)
            {
                for (int j = 0; j < num_units; j++)
                {
                    incr = scale_units[j] * Math.Pow(10D, (double)(i));
                    if (incr != 0D)
                        minout = incr * Math.Floor(min / incr);
                    for (int k = 0; k < num_divs; k++)
                    {
                        maxout = minout + incr * scale_div[k];
                        if (maxout > max)
                        {
                            min = minout;
                            max = maxout;
                            spacing = incr;
                            return;
                        }
                    }
                }
            }
            min     = minout;
            max     = maxout;
            spacing = incr;
            return;
        }
/********************************************************************************************/
        private void MyRegression(List<PointD> pointList, ref List<PointD> outList)
        {
            int N = pointList.Count;
            double[] XY = new double[N];
            double[] XX = new double[N];
            for (int i = 0; i < N; i++)
            {
                XY[i] = pointList[i].X * pointList[i].Y;
                XX[i] = pointList[i].X * pointList[i].X;
            }
            double sumX = 0D;
            double sumY = 0D;
            double sumXY = 0D;
            double sumXX = 0D;
            for (int i = 0; i < N; i++)
            {
                sumX += pointList[i].X;
                sumY += pointList[i].Y;
                sumXY += XY[i];
                sumXX += XX[i];
            }
            double numerator = (double)(N) * sumXY - (sumX * sumY);
            double denominator = ((double)(N) * sumXX) - (sumX * sumX);
            double b = numerator / denominator; // Slope
            double a = (sumY - (b * sumX)) / (double)(N); // Intercept
            double mean = sumXX / (double)(N);
            double x_avg = sumX / (double)(N);
            double y_avg = sumY / (double)(N);
            double[] xSet = new double[N];
            double[] ySet = new double[N];
            for (int i = 0; i < N; i++)
            {
                PointD p = new PointD();
                double x = pointList[i].X;
                p.X = x;
                double y = a + (b * pointList[i].X);
                p.Y = y;
                outList.Add(p);
            }
        }
/********************************************************************************************/
        private double[] RaisePower(double[] data, int power)
        {
            double[] result = new double[data.GetLength(0)];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                result[i] = Math.Pow(data[i], power);
            }
            return result;
        }
/********************************************************************************************/
        private int MyPoly(int degree, List<PointD> pointList, ref List<PointD> outList )
        {
            double md = (double) degree;
            double dr = md;
            double freq_dr = dr;
            double coeff = 90D;
            int N = pointList.Count;
            double [] p = new double[30];
            double [] b = new double[60];
            double [] c = new double[60];
            double [,] dx = new double[15,15];
            for ( int i=0; i<30; i++ )
                p[i] = 0.0;
            for ( int i=0; i<60; i++ )
                b[i] = 0.0;
            for ( int i=0; i<60; i++ )
                c[i] = 0.0;
            for ( int i=0; i<15; i++ )
                {
                for ( int j=0; j<15; j++ )
                    dx[i,j] = 0.0;
                }
            for (int i = 0; i < N; i++)
            {
                double fx = pointList[i].X;
                double fy = pointList[i].Y;
                dx[0,0]  += 1D;
                dx[0, (int)md + 1] = dx[0, (int)md + 1] + fy;
                dx[(int)md + 1, (int)md + 1] = +dx[(int)md + 1, (int)md + 1] + fy * fy;
                for (int l = 1; l <= md; l++)
                {
                    p[l] = p[l] + Math.Pow(fx, (double)l);
                    dx[l, (int)md + 1] = dx[l, (int)md + 1] + fy * Math.Pow(fx, (double)l);
                    p[(int)md + l] = p[(int)md + l] + Math.Pow(fx, (double)(md + l));
                }
            }
            for (int i = 0; i <= md; i++)
            {
                if (i == 0)
                {
                    for (int j = i + 1; j <= md; j++)
                        dx[i, j] = p[i + j];
                }
                else
                {
                    for (int j = i; j <= md; j++)
                        dx[i, j] = p[i + j];
                }
            }
            double fn = dx[0, 0];
            double vx = (dx[1,1] - dx[0,1] * dx[0,1]/fn) / (fn-1D);
            double vy  = (dx[(int)md+1,(int)md+1] - dx[0,(int)md+1]*dx[0,(int)md+1]/fn) / (fn-1.0);
            double n = md;
            double nobs = dx[0,0];
            double tss = dx[(int)(n+1),(int)(n+1)] - dx[0,(int)n+1] * dx[0,(int)n+1] / dx[0,0];
            double rs = dx[(int)n+1,(int)n+1];
            dx[0,0] = Math.Sqrt ( dx[0,0]);
            if ( dx[0,0] == 0D )
                return -1;
            for ( int i=1; i<=(int)n+1; i++ )
                dx[0,i] = dx[0,i] / dx[0,0];
            for ( int i=1; i<=n; i++ )
            {
                for ( int j=i; j<=n+1; j++ )
                {
                    if ( i == j )
                    {
                        for ( int k=0; k<=i-1; k++ )
                            dx[i,i] = dx[i,i] - dx[k,i] * dx[k,i];
                        if ( dx[i,i] <= 0D )
                            return -1; // Maximum Degree Exceeded
                        dx[i,i] = Math.Sqrt(dx[i,i] );
                    }
                    else
                    {
                        for ( int k=0; k<=i-1; k++ )
                            dx[i,j] = dx[i,j] - dx[k,i] * dx[k,j];
                        dx[i,j] = dx[i,j] / dx[i,i];
                    }
                }
            }
            double dss = tss;
            double css = 0D;
            for (int i = 0; i <= n; i++)
            {
                css = css + dx[i, (int)n + 1] * dx[i, (int)n + 1];
                dss = dss - dx[i, (int)n + 1] * dx[i, (int)n + 1];
            }
            if (dr > md)
                dr = md;
            double rss = rs;
            n = dr;
            for ( int i=0; i<=n; i++ )
                rss = rss - dx[i,(int)md+1] * dx[i,(int)md+1];
            if (rss < 0D && (rss > -1e-09))
                rss = 1e-09;
            double r2 = (tss - rss) / tss;
            for (int i = 0; i <= md; i++)
                dx[i, i] = 1D / dx[i, i];
            for (int i = (int)md - 1; i >= 0; i--)
            {
                for (int j = (int)md; j >= i + 1; j--)
                {
                    double dum = 0D;
                    for (int k = i + 1; k <= j; k++)
                        dum = dum + dx[i, k] * dx[k, j];
                    dx[i, j] = -1D * dum * dx[i, i];
                }
            }
            for (int i = 0; i <= n; i++)
            {
                for (int j = i; j <= n; j++)
                {
                    b[i] = b[i] + dx[i, j] * dx[j, (int)md + 1];
                    c[i] = c[i] + dx[i, j] * dx[i, j];
                }
                c[i] = Math.Sqrt(c[i] * rss / (double)(nobs - n - 1));
            }
            double pp = coeff;
            if (pp <= 0D || pp >= 100D)
                pp = 90D;
            double p1 = (1D - pp / 100D) / 2D;
            double v = Math.Sqrt(Math.Log10(1D / (p1 * p1)));
            double z = v - (2.515517D + 0.802853D * v + 0.010328D * v * v) / 
                (1D + 1.432788D * v + 0.189269D * v * v + 0.001308D * v * v * v);
            double m = nobs - 1D - n;
            double t = z + (Math.Pow (z,3D)+z)/(4D*m) + 
                (5D*Math.Pow(z,5D)+16D*Math.Pow(z,3D)+3D*z) / (96D*m*m) +
                (3D*Math.Pow(z,7D)-19D*Math.Pow(z,5D)+17D*Math.Pow(z,3D)-15D * z) /
                (384D*Math.Pow(m,(double)3)) +
                (79.0*Math.Pow(z,(double)9)+776.0*Math.Pow(z,(double)7)+1482.0*Math.Pow(z,(double)5)-
                1920.0*Math.Pow(z,(double)3)-945.0*z) /
                (92160.0*Math.Pow(m,(double)4) );

            double ser = Math.Sqrt(rss / (nobs - n - 1D));
            double xmin = XMinPoint;
            double xmax = XMaxPoint;
            double spacing = 5D;
            adjust_scale(ref xmin, ref xmax, ref spacing);
            double total_xscale = xmax - xmin;
            double ymin = YMinPoint;
            double ymax = YMaxPoint;
            spacing = 5D;
            adjust_scale(ref ymin, ref ymax, ref spacing);
            double saveyc = 0D;
            double savey = 1000000D;
            double savex = 0D;
            for (double fx = xmin; fx <= xmax; fx = fx + total_xscale / 100D)
            {
                double fy = b[0];
                for (int j = 1; j <= dr; j++)
                    fy = fy + b[j] * Math.Pow(fx, (double)j);
                if (Math.Abs(fy - saveyc) < savey)
                {
                    savey = Math.Abs(fy - saveyc);
                    savex = fx;
                }
                saveyc = fy;
                PointD point = new PointD();
                point.X = fx;
                point.Y = fy;
                outList.Add(point);
            }
            return 0;
        }
        /********************************************************************************************/
        private double calc_coeff(List<PointD> pointList)
        {
            int N = pointList.Count;
            double[] XY = new double[N];
            double[] XX = new double[N];
            for (int i = 0; i < N; i++)
            {
                XY[i] = pointList[i].X * pointList[i].Y;
                XX[i] = pointList[i].X * pointList[i].X;
            }
            double sumX = 0D;
            double sumY = 0D;
            double sumXY = 0D;
            double sumXX = 0D;
            for (int i = 0; i < N; i++)
            {
                sumX += pointList[i].X;
                sumY += pointList[i].Y;
                sumXY += XY[i];
                sumXX += XX[i];
            }
            double numerator = (double)(N) * sumXY - (sumX * sumY);
            double denominator = ((double)(N) * sumXX) - (sumX * sumX);
            double b = numerator / denominator; // Slope
            double a = (sumY - (b * sumX)) / (double)(N); // Intercept
            double mean = sumXX / (double)(N);
            double x_avg = sumX / (double)(N);
            double y_avg = sumY / (double)(N);
            double[] xSet = new double[N];
            double[] ySet = new double[N];
            double[] x1Set = new double[N];
            double[] x2Set = new double[N];
            for (int i = 0; i < N; i++)
            {
                double x = pointList[i].X;
                double y = pointList[i].Y;
                xSet[i] = x - x_avg;
                ySet[i] = y - y_avg;
                x1Set[i] = xSet[i] * ySet[i];
                x2Set[i] = xSet[i] * xSet[i];
            }
            double sum_x1y1 = 0D;
            double sum_x12 = 0D;
            for (int i = 0; i < N; i++)
            {
                sum_x1y1 += x1Set[i];
                sum_x12 += x2Set[i];
            }
            double coeff = sum_x1y1 / sum_x12;
            return coeff;
        }
/********************************************************************************************/
        private struct Operators
        {
            public List<double> Operatos;

        }

        private struct LineC
        {
            public PointD sPoint;
            public PointD ePoint;
        }

        private List<PointD> points = new List<PointD>();
        Graphics g;
        //Define a List of Operators (to hold a list of Lagrange operators in each od it's elements)
        List<Operators> OpList;//= new List<Operators>();
        //A list of X coordinates which we will compute the Lagrange operators according to and they will be
        //the X coordinates of the fitted curve.
        //A list of Y coordinates which is computed according to Lagrange operators and they will be 
        //the Y coordiantes of the fitted curve.
        List<double> Xs;//= new List<float>();
        List<double> Ys;//= new List<float>();
        bool curveRendered = false;
        List<LineC> lineList = new List<LineC>();
        List<PointD> intersectedPoints = new List<PointD>();
       
        private void Regression(List<PointD> pointList, ref List<PointD> outList )
        {
            OpList = new List<Operators>();
            Xs = new List<double>();
            Ys = new List<double>();

            try
            {
                if (pointList.Count > 0)
                {
                    //compute lagrange operator for each X coordinate
                    for (int x = 1; x < 2000; x++)
                    {
                        //list of float to hold the Lagrange operators
                        List<double> L = new List<double>();
                        //Init the list with 1's
                        for (int i = 0; i < pointList.Count; i++)
                        {
                            L.Add(1);
                        }
                        for (int i = 0; i < L.Count; i++)
                        {
                            for (int k = 0; k < pointList.Count; k++)
                            {
                                if (i != k)
                                 L[i] *= (double)(x - pointList[k].X) / (pointList[i].X - pointList[k].X);
                            }
                        }
                        Operators o = new Operators();
                        o.Operatos = L;
                        OpList.Add(o);
                        Xs.Add(x);

                    }

                    //Computing the Polynomial P(x) which is y in our curve
                    foreach (Operators O in OpList)
                    {
                        double y = 0;
                        for (int i = 0; i < pointList.Count; i++)
                        {
                            y += O.Operatos[i] * pointList[i].Y;
                        }

                        Ys.Add(y);
                    }

                    //Drawing the curve in the simplest way
                    for (int i = 0; i < pointList.Count; i++)
                    {
                        PointD p = new PointD();
                        p.X = Xs[i];
                        p.Y = Ys[i];
                        outList.Add(p);
                    }
                    
                }
                else
                {
                    if (MessageBox.Show("Pick some points", "Lagrange curve fitting", MessageBoxButtons.OK,MessageBoxIcon.Information) == DialogResult.OK)
                    {
                    }
                }
            }
            catch(Exception ex)
            {
                return;
            }
        }
/***********************************************************************************************/
        string XAxis_ScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
        {
            long ldate = (long)val;

            string sdate = G1.days_to_date(ldate);
            DateTime dt = sdate.ObjToDateTime();
            string date = dt.Month.ToString("D2") + "/" + (dt.Year % 100).ToString("D2");
            return date;
        }
/***********************************************************************************************/
        private void checkBox_smoothLines_CheckedChanged(object sender, EventArgs e)
        { // Smooth Lines
            UpdatePlot(zedPlot);
        }
/***********************************************************************************************/
        private void checkBox_showTrendLines_CheckedChanged(object sender, EventArgs e)
        { // Show Trend Lines
            UpdatePlot(zedPlot);
        }
/***********************************************************************************************/
        private void multiCheckBox_CheckedChanged(object sender, EventArgs e)
        { // Split the plots up
            UpdatePlot(zedPlot);
        }
/***********************************************************************************************/
        private void btnPlot_Click(object sender, EventArgs e)
        { // Replot probably after changing dates
            UpdatePlot(zedPlot);
        }
/***********************************************************************************************/
        private void chkRegress_CheckedChanged(object sender, EventArgs e)
        { // Plot Polynomial Regression
            UpdatePlot(zedPlot);
        }
/***********************************************************************************************/
        private void txtDegree_TextChanged(object sender, EventArgs e)
        { // Plot Ploynomial Regression to the Nth degree
            UpdatePlot(zedPlot);
        }
/***********************************************************************************************/
        private void PlotData_Resize(object sender, EventArgs e)
        { // Size is Changing
            int width = this.Width - this.panel2.Width;
            this.panel1.Width = width;
            UpdatePlot(zedPlot);
        }
/***********************************************************************************************/
    }
}
