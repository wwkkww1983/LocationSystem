﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DbModel.Location.AreaAndDev;
using DbModel.Tools;
using Location.IModel;

namespace WPFClientControlLib
{
    /// <summary>
    /// Interaction logic for AreaCanvas.xaml
    /// </summary>
    public partial class AreaCanvas : UserControl
    {
        public AreaCanvas()
        {
            InitializeComponent();
        }

        public void SelectArea(Area area)
        {
            if (area == null) return;
            if (AreaDict.ContainsKey(area.Id))
            {
                ClearSelect();
                FocusRectangle(AreaDict[area.Id]);
                LbState.Content = "";
            }
            else
            {
                ClearSelect();
                LbState.Content = "未找到区域:" + area.Name;
            }
        }

        public void SelectDev(DevInfo dev)
        {
            if (dev == null) return;
            if (DevDict.ContainsKey(dev.Id))
            {
                ClearSelect();
                FocusRectangle(DevDict[dev.Id]);
                LbState.Content = "";
            }
            else
            {
                ClearSelect();
                LbState.Content = "未找到设备:" + dev.Name;
            }
        }

        public void FocusRectangle(Shape rect)
        {
            foreach(Shape shape in Canvas1.Children)
            {
                if(shape!= rect)
                    SetShapeStrokeDash(shape);
            }

            SelectedRect = rect;
            SelectedRect.Stroke = Brushes.Red;
            SelectedRect.StrokeThickness = 2;
            SelectedRect.Focus();
            //VisualTreeHelper.sc
            ScrollViewer1.ScrollToHorizontalOffset(SelectedRect.Margin.Left);
            ScrollViewer1.ScrollToVerticalOffset(SelectedRect.Margin.Top);
        }

        public Shape SelectedRect { get; set; }

        public Area Current { get; set; }

        private void InitCbScale(int scale)
        {
            CbScale.SelectionChanged -= CbScale_SelectionChanged;
            CbScale.SelectedItem = scale;
            CbScale.SelectionChanged += CbScale_SelectionChanged;
        }

        private void InitCbDevSize(double[] list,double item)
        {
            CbDevSize.SelectionChanged -= CbDevSize_SelectionChanged;
            CbDevSize.ItemsSource = list;
            CbDevSize.SelectedItem = item;
            CbDevSize.SelectionChanged += CbDevSize_SelectionChanged;
        }

        public void ShowArea(Area area)
        {
            try
            {
                CbView.SelectionChanged -= CbView_OnSelectionChanged;
                CbView.SelectionChanged += CbView_OnSelectionChanged;

                
                if (area == null) return;

                if (area.Parent.Name == "根节点") //电厂
                {
                    Current = area;
                    int scale = 2;
                    DevSize = 2;
                    DrawPark(area, scale, DevSize);
                    InitCbScale(scale);

                    InitCbDevSize(new double[] { 0.5, 1, 2, 3 }, DevSize);
                }
                else if (area.Type == AreaTypes.楼层)
                {
                    Current = area;
                    int scale = 20;
                    DevSize = 0.2;
                    DrawFloor(area, scale, DevSize);
                    InitCbScale(scale);

                    InitCbDevSize(new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 }, DevSize);
                }
                else
                {
                    SelectArea(area);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }


        private void ClearSelect()
        {
            foreach (Shape shape in Canvas1.Children)
            {
                shape.StrokeDashArray = null;
            }
            if (SelectedRect != null)
            {
                SelectedRect.Stroke = Brushes.Black;
                SelectedRect.StrokeThickness = 1;
            }
        }

        public Dictionary<int, Shape> AreaDict = new Dictionary<int, Shape>();
        public Dictionary<int, Rectangle> DevDict = new Dictionary<int, Rectangle>();

        public double Scale = 1;

        private void AddZeroPoint()
        {
            /*
             * <Ellipse Canvas.Left="60" Canvas.Top="80" Width="100" Height="100"

　　Fill="Blue" Opacity="0.5" Stroke="Black" StrokeThickness="3"/>
             */
            Ellipse ellipse = new Ellipse();
            ellipse.Margin = new Thickness(Margin/2, Margin/2, 0, 0);
            ellipse.Width = 5;
            ellipse.Height = 5;
            ellipse.Fill = Brushes.Orange;
            ellipse.Stroke = Brushes.Blue;
            ellipse.StrokeThickness = 1;
            Canvas1.Children.Add(ellipse);

        }

        private void DrawFloor(Area area,double scale,double devSize)
        {
            Clear();

            Bound bound = area.InitBound;
            if (bound == null) return;

            Scale = scale;

            Margin = 10;
            OffsetX = -Margin/2;
            OffsetY = -Margin/2;
            Canvas1.Width = (bound.MaxX+ Margin) * scale ;
            Canvas1.Height = (bound.MaxY+ Margin) * scale;

            

            AddAreaRect(area, null, scale);

            if (area.Children != null)
                foreach (var level1Item in area.Children) //机房
                {
                    AddAreaRect(level1Item, null, scale, true);
                }

            if(area.LeafNodes!=null)
                foreach (var dev in area.LeafNodes)
                {
                    AddDevRect(dev, scale, devSize);
                }

            AddZeroPoint();
        }

        private void Clear()
        {
            Canvas1.Children.Clear();
            AreaDict.Clear();
            SelectedRect = null;
            OffsetX = 0;
            OffsetY = 0;
        }

        public double OffsetX = 0;
        public double OffsetY = 0;

        public double Margin = 20;

        private void DrawPark(Area area,int scale,double devSize)
        {
            Clear();

            Bound bound = area.InitBound;
            //if (bound == null)
            //{
            //    bound=area.CreateBoundByChildren();
            //}
            if (bound == null) return;

            //bound=area.SetBoundByDevs();

            Margin = 20;

            OffsetX = bound.MinX - Margin;
            OffsetY = bound.MinY - Margin;

            Canvas1.Width = (bound.MaxX - OffsetX + Margin) * scale;
            Canvas1.Height =(bound.MaxY - OffsetY + Margin) *scale;

            AddAreaRect(area,null, scale);

            if (area.Children != null)
                foreach (var level1Item in area.Children) //建筑群
                {
                    AddAreaRect(level1Item, area, scale);

                    if (level1Item.Children != null)
                        foreach (var level2Item in level1Item.Children) //建筑
                        {
                            AddAreaRect(level2Item, level1Item, scale);
                        }
                }
            if(area.LeafNodes!=null)
                foreach (var dev in area.LeafNodes)
                {
                    AddDevRect(dev, scale, devSize);
                }
        }

        private void AddDevRect(DevInfo dev,double scale, double size = 2)
        {
            double x = (dev.PosX - OffsetX) * scale;
            double y = (dev.PosZ - OffsetY) * scale;
            //if (ViewMode == 0)
            //    y = Canvas1.Height - size * scale - y; //上下颠倒一下，不然就不是CAD上的上北下南的状况了
            Rectangle devRect = new Rectangle()
            {
                Margin = new Thickness(x, y, 0, 0),
                Width = size * scale,
                Height = size * scale,
                Fill = Brushes.Blue,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = dev,
                ToolTip = dev.Name
            };
            DevDict[dev.Id] = devRect;
            devRect.MouseDown += DevRect_MouseDown;
            devRect.MouseEnter += DevRect_MouseEnter;
            devRect.MouseLeave += DevRect_MouseLeave;
            Canvas1.Children.Add(devRect);
            
        }

        private void DevRect_MouseLeave(object sender, MouseEventArgs e)
        {
            UnSelectRectangle(sender as Rectangle);
        }

        private void DevRect_MouseEnter(object sender, MouseEventArgs e)
        {
            //SelectRectangle(sender as Rectangle);
            Rectangle rect = sender as Rectangle;
            DevInfo dev = rect.Tag as DevInfo;
            LbState.Content = GetDevText(dev);

            rect.Stroke = Brushes.Red;
            SelectedRect = rect;
        }

        private string GetDevText(DevInfo dev)
        {
            return string.Format("[{0}]({1},{2})", dev.Name, dev.PosX, dev.PosZ); 
        }

        private void DevRect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            DevInfo dev = rect.Tag as DevInfo;
            LbState.Content = GetDevText(dev);

            if (DevSelected != null)
            {
                DevSelected(dev);
            }
        }

        public event Action<DevInfo> DevSelected;

        

        private void AddAreaRect(Area area, Area parent, double scale = 1,bool isTransparent = false)
        {
            if (area == null) return;
            Bound bound = area.InitBound;
            if (bound == null) return;

            //{
            //    double x = (bound.MinX - OffsetX) * scale;
            //    double y = (bound.MinY - OffsetY) * scale;
            //    //if (ViewMode == 0)
            //    //    y = Canvas1.Height - bound.GetHeight() * scale - y; //上下颠倒一下，不然就不是CAD上的上北下南的状况了
            //    if (bound.IsRelative && parent != null)
            //    {
            //        x += parent.InitBound.MinX * scale;
            //        y -= parent.InitBound.MinY * scale;
            //    }

            //    Rectangle areaRect = new Rectangle()
            //    {
            //        Margin = new Thickness(x, y, 0, 0),
            //        Width = bound.GetWidth() * scale,
            //        Height = bound.GetHeight() * scale,
            //        Fill = Brushes.LightGoldenrodYellow,
            //        Stroke = Brushes.Black,
            //        StrokeThickness = 1,
            //        Tag = area,
            //        ToolTip = area.Name
            //    };
            //    AreaDict[area.Id] = areaRect;
            //    areaRect.MouseEnter += AreaRect_MouseEnter;
            //    areaRect.MouseLeave += AreaRect_MouseLeave;
            //    //Canvas1.Children.Add(areaRect);
            //}

            {
                //< Polygon Fill = "AliceBlue" StrokeThickness = "5" Stroke = "Green" Points = "40,10 70,80 10,50" />

                Polygon polygon = new Polygon();
                if (isTransparent)
                {
                    polygon.Fill = Brushes.Transparent;
                }
                else
                {
                    polygon.Fill = Brushes.AliceBlue;
                }
                
                polygon.Stroke = Brushes.Black;
                polygon.StrokeThickness = 1;
                polygon.MouseEnter += Polygon_MouseEnter;
                polygon.MouseLeave += Polygon_MouseLeave;
                polygon.Tag = area;

                if (area.Type == AreaTypes.范围)
                {
                    polygon.Fill = Brushes.Transparent;
                    SetShapeStrokeDash(polygon);
                }

                foreach (var item in bound.GetPoints2D())
                {
                    double x = (item.X - OffsetX) * scale;
                    double y = (item.Y - OffsetY) * scale;
                    //if (ViewMode == 0)
                    //    y = Canvas1.Height - /*bound.GetHeight() * scale*/ - y; //上下颠倒一下，不然就不是CAD上的上北下南的状况了
                    //if (bound.IsRelative && parent != null)
                    //{
                    //    x += parent.InitBound.MinX * scale;
                    //    y -= parent.InitBound.MinY * scale;
                    //}
                    polygon.Points.Add(new System.Windows.Point(x, y));
                }

                AreaDict[area.Id] = polygon;
                Canvas1.Children.Add(polygon);
            }
        }

        private void SetShapeStrokeDash(Shape shape)
        {
            shape.StrokeDashArray = new DoubleCollection() { 2, 3 };
            shape.StrokeDashCap = PenLineCap.Triangle;
            shape.StrokeEndLineCap = PenLineCap.Square;
            shape.StrokeStartLineCap = PenLineCap.Round;
        }

        private void Polygon_MouseLeave(object sender, MouseEventArgs e)
        {
            UnSelectRectangle(sender as Shape);
        }

        private void Polygon_MouseEnter(object sender, MouseEventArgs e)
        {
            SelectRectangle(sender as Shape);
        }

        private void AreaRect_MouseLeave(object sender, MouseEventArgs e)
        {
            UnSelectRectangle(sender as Shape);
        }

        private void AreaRect_MouseEnter(object sender, MouseEventArgs e)
        {
            SelectRectangle(sender as Shape);
        }

        private void SelectRectangle(Shape rect)
        {
            IName entity = rect.Tag as IName;
            LbState.Content = "" + entity.Name;
            rect.Stroke = Brushes.Red;
            rect.StrokeThickness = 2;

            //Canvas1.Children.Remove(rect);
            //Canvas1.Children.Add(rect);

            SelectedRect = rect;
        }

        private void UnSelectRectangle(Shape rect)
        {
            IName entity = rect.Tag as IName;
            LbState.Content = "" + entity.Name;

            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 1;
            SelectedRect = null;
        }

        private void CbScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }



        private void Refresh()
        {
            try
            {
                int scale = (int)CbScale.SelectedItem;
                Area area = Current;
                if (area == null) return;
                if (area.ParentId == 1) //电厂
                {
                    DrawPark(area, scale, DevSize);
                }
                else if (area.Type == AreaTypes.楼层)
                {
                    DrawFloor(area, scale, DevSize);
                }
                else
                {
                    SelectArea(area);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }


        private void CbDevSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DevSize = (double)CbDevSize.SelectedItem;
            
            RefreshDevs();
        }

        private void RefreshDevs()
        {
            Refresh();
        }

        public double DevSize { get; set; }

        private void AreaCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        public void Init()
        {
            CbScale.ItemsSource = new int[] { 1, 2, 3, 4, 5, 10, 20, 30, 40, 50 };
            CbScale.SelectedIndex = 0;

            
        }

        public void ShowArchors(Archor[] archors)
        {
            
        }

        public void ShowDevs(DevInfo[] devs)
        {
            
        }

        public void ShowPersons()
        {
            
        }

        private void CbView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewMode = CbView.SelectedIndex;
            if (ViewMode == 0)
            {
                ScaleTransform1.ScaleY = -1;
            }
            else
            {
                ScaleTransform1.ScaleY = 1;
            }
            Refresh();
        }

        public int ViewMode { get; set; }
    }
}