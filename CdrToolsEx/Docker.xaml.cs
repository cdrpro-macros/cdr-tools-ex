using System;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml;
using Corel.Interop.VGCore;
using Microsoft.Win32;
using Color = Corel.Interop.VGCore.Color;
using Shape = Corel.Interop.VGCore.Shape;
using Style = Corel.Interop.VGCore.Style;
using TextRange = Corel.Interop.VGCore.TextRange;




/*
 * 1 - distribute objects
 * 2 - crop marks - color
 * 3 - crop marks - each object
 * 4 - step & repeate - replace 'Use Step as value for spacing' to Space field
 * 5 - Use outline for get/set size
 * 6 - Shift/Ctrl + get size = get only width/height
 */



namespace CdrToolsEx
{
    public partial class Docker : UserControl
    {

        Corel.Interop.VGCore.Application dApp = null;
        private cdrReferencePoint _rPoint;

        private string _uPath;
        private bool _loadByCode;
        private Style _findStyle = null;
        private Style _replaceStyle = null;
        private Color _marksColor = null;

        public const string MName = "CdrToolsEx";
        public const string MVer = "2.1";
        public const string MYear = "2019";
        public const string MDate = "26.07.2019";
        public const string MWebSite = @"https://cdrpro.ru";
        public const string MWebPage = @"https://cdrpro.ru/en/macros/cdrtools-ex/";
        public const string MEmail = "sancho@cdrpro.ru";

        public Docker() { InitializeComponent(); }
        public Docker(object app)
        {
            try
            {
                InitializeComponent();
                dApp = (Corel.Interop.VGCore.Application)app;

                Load1();

                if (!File.Exists(_uPath)) CreateXml();
                UpdateFile();

                _rPoint = cdrReferencePoint.cdrCenter;
                rpC.IsChecked = true;

                _loadByCode = false;
                LoadSettings();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Load1()
        {
            try
            {
                string uFolderPath = Environment.GetEnvironmentVariable("APPDATA") + @"\Corel\" + MName;
                if (!Directory.Exists(uFolderPath)) Directory.CreateDirectory(uFolderPath);
                _uPath = uFolderPath + @"\Settings.xml";

                /* Load languages */
            }
            catch (Exception err) { MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void UpdateFile()
        {
            try
            {
                var xDoc = new XmlDocument();
                xDoc.Load(_uPath);

                var opt = xDoc.SelectSingleNode(@"/App/Options");
                if (opt == null) return;

                /* 1.1 */
                /* StepRepeat */
                var n = xDoc.SelectSingleNode(@"/App/Options/StepRepeat");
                if (n != null)
                {
                    var nn = xDoc.CreateElement("exStepRepeat");
                    nn.InnerText = "1";
                    opt.AppendChild(nn);
                    opt.RemoveChild(n);
                }

                n = xDoc.SelectSingleNode(@"/App/Options/StepRepeatUseOutline");
                if (n == null)
                {
                    var nn = xDoc.CreateElement("StepRepeatUseOutline");
                    nn.InnerText = "1";
                    opt.AppendChild(nn);
                }

                /* 1.2 */
                /* Add CropMarks settings */
                n = xDoc.SelectSingleNode(@"/App/Options/exCropMarks");
                if (n == null)
                {
                    var nn = xDoc.CreateElement("exCropMarks"); nn.InnerText = "1"; opt.AppendChild(nn);
                    nn = xDoc.CreateElement("CropMarksLenght"); nn.InnerText = "5"; opt.AppendChild(nn);
                    nn = xDoc.CreateElement("CropMarksOffset"); nn.InnerText = "2"; opt.AppendChild(nn);
                    nn = xDoc.CreateElement("CropMarksStrokeWeight"); nn.InnerText = "0.2"; opt.AppendChild(nn);
                    nn = xDoc.CreateElement("CropMarksUseOutline"); nn.InnerText = "1"; opt.AppendChild(nn);
                }

                /* 2.0 */
                /* Distribute */
                n = xDoc.SelectSingleNode(@"/App/Options/exDistribute");
                if (n == null)
                {
                    var nn = xDoc.CreateElement("exDistribute"); nn.InnerText = "0"; opt.AppendChild(nn);
                    nn = xDoc.CreateElement("DistributeSpace"); nn.InnerText = "0"; opt.AppendChild(nn);
                    nn = xDoc.CreateElement("DistributeUseOutline"); nn.InnerText = "0"; opt.AppendChild(nn);
                    /* CropMarks */
                    nn = xDoc.CreateElement("CropMarksEachObject"); nn.InnerText = "0"; opt.AppendChild(nn);
                    nn = xDoc.CreateElement("CropMarksColor"); nn.InnerText = "CMYK255,USER,0,0,0,255,100,cccd19cb-4675-4a5e-8bda-d0bbbaab8af0"; opt.AppendChild(nn);
                }

                n = xDoc.SelectSingleNode(@"/App/Options/UseStep");
                if (n != null)
                {
                    var nn = xDoc.CreateElement("StepRepeatSpace"); nn.InnerText = "0"; opt.AppendChild(nn);
                    opt.RemoveChild(n);
                }

                n = xDoc.SelectSingleNode(@"/App/Options/TransformUseOutline");
                if (n == null)
                {
                    var nn = xDoc.CreateElement("TransformUseOutline"); nn.InnerText = "0"; opt.AppendChild(nn);
                }

                xDoc.Save(_uPath);
            }
            catch (Exception err) { MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CreateXml()
        {
            Stream inFile = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("CdrToolsEx.DefaultSettings.xml");
            var xDoc = new XmlDocument(); xDoc.Load(inFile); xDoc.Save(_uPath);
        }

        private void LoadSettings()
        {
            try
            {
                var xDoc = new XmlDocument();
                xDoc.Load(_uPath);

                var xList = xDoc.SelectSingleNode(@"/App/Options");
                if (xList == null)
                {
                    MessageBox.Show("Can't load settings", MName, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _loadByCode = true;
                foreach (XmlNode n in xList.ChildNodes)
                {
                    switch (n.Name)
                    {
                        case "Step": tbStep.Text = n.InnerText; break;
                        case "exIncreaseAndDecrease": exIncreaseAndDecrease.IsExpanded = ValToBool(n.InnerText); break;
                        case "exTransformations": exTransformations.IsExpanded = ValToBool(n.InnerText); break;
                        case "exStepRepeat": exStepRepeat.IsExpanded = ValToBool(n.InnerText); break;
                        case "exDistribute": exDistribute.IsExpanded = ValToBool(n.InnerText); break;
                        case "exCropMarks": exCropMarks.IsExpanded = ValToBool(n.InnerText); break;
                        case "exOffsetPath": exOffsetPath.IsExpanded = ValToBool(n.InnerText); break;
                        case "exReplaceText": exReplaceText.IsExpanded = ValToBool(n.InnerText); break;

                        case "Width": tbWidth.Text = n.InnerText; break;
                        case "Height": tbHeight.Text = n.InnerText; break;
                        case "Rotate": tbRotate.Text = n.InnerText; break;
                        case "TransformUseOutline": cbTransformUseOutline.IsChecked = ValToBool(n.InnerText); break;

                        case "PropertyType": cbPropertyType.Text = n.InnerText; break;

                        case "HorizontalCount": tbHorizontalCount.Text = n.InnerText; break;
                        case "VerticalCount": tbVerticalCount.Text = n.InnerText; break;
                        case "StepRepeatUseOutline": cbStepRepeatUseOutline.IsChecked = ValToBool(n.InnerText); break;
                        case "StepRepeatSpace": tbStepRepeatSpace.Text = n.InnerText; break;

                        case "DistributeSpace": tbDistributeSpace.Text = n.InnerText; break;
                        case "DistributeUseOutline": cbDistributeUseOutline.IsChecked = ValToBool(n.InnerText); break;

                        case "CropMarksLenght": tbCropMarksLenght.Text = n.InnerText; break;
                        case "CropMarksOffset": tbCropMarksOffset.Text = n.InnerText; break;
                        case "CropMarksStrokeWeight": tbCropMarksStrokeWeight.Text = n.InnerText; break;
                        case "CropMarksUseOutline": cbCropMarksUseOutline.IsChecked = ValToBool(n.InnerText); break;
                        case "CropMarksEachObject": cbCropMarksRange.IsChecked = ValToBool(n.InnerText); break;
                        case "CropMarksColor": _marksColor = dApp.CreateColor(n.InnerText); ApplyColorUI(); break;

                        case "CornerType": cbCornerType.Text = n.InnerText; break;
                        case "Offset": tbOffset.Text = n.InnerText; break;
                        case "MiterLimit": tbMiterLimit.Text = n.InnerText; break;
                        case "InvertColor": cbInvertColor.IsChecked = ValToBool(n.InnerText); break;
                    }
                }
                DefMarksApply();
                _loadByCode = false;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
                _loadByCode = false;
            }
        }

        private void ApplyColorUI()
        {
            var nRGB = dApp.CreateRGBColor(0, 0, 0);
            nRGB.CopyAssign(_marksColor);

            if (nRGB.Type != cdrColorType.cdrColorRGB) nRGB.ConvertToRGB();
            btMarksColor.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)nRGB.RGBRed, (byte)nRGB.RGBGreen, (byte)nRGB.RGBBlue));
            btMarksColor.ToolTip = _marksColor.Name + "\n" + _marksColor.Name[true];
        }

        private void SelectAll(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb.Text.Length > 0) tb.SelectAll();
        }

        private void SaveKeyVal(string keyName, string keyVal)
        {
            var xDoc = new XmlDocument();
            xDoc.Load(_uPath);
            var n = xDoc.SelectSingleNode(@"/App/Options/" + keyName);
            if (n != null)
            {
                n.InnerText = keyVal;
                xDoc.Save(_uPath);
            }
        }

        private bool ValToBool(string val) { return val == "1"; }
        private string BoolToVal(bool b) { return b ? "1" : "0"; }

        private void ChangeTextBox(object sender, KeyEventArgs e)
        {
            var tb = (TextBox)sender;
            var d = str2dbl(tb.Text);
            if (d < 0) { d = 0; tb.Text = d.ToString(CultureInfo.InvariantCulture); }

            // add code...
            SaveKeyVal("Step", d.ToString(CultureInfo.InvariantCulture));
        }

        private void ExpanderExpanded(object sender, RoutedEventArgs e) { ExpanderEx(sender, "1"); }
        private void ExpanderCollapsed(object sender, RoutedEventArgs e) { ExpanderEx(sender, "0"); }
        private void ExpanderEx(object sender, string state)
        {
            if (_loadByCode) return;
            var ex = (Expander)sender;
            SaveKeyVal(ex.Name, state);
        }

        private void boostStart(string undo = "")
        {
            if (undo.Length > 0) dApp.ActiveDocument.BeginCommandGroup(undo);
            dApp.Optimization = true;
            dApp.EventsEnabled = false;
            dApp.ActiveDocument.SaveSettings();
            dApp.ActiveDocument.PreserveSelection = false;
        }

        private void boostFinish(bool endUndo = false)
        {
            dApp.ActiveDocument.PreserveSelection = true;
            dApp.ActiveDocument.ResetSettings();
            dApp.EventsEnabled = true;
            dApp.Optimization = false;
            if (endUndo) dApp.ActiveDocument.EndCommandGroup();
            dApp.ActiveDocument.ActiveWindow.Refresh();
            dApp.Refresh();
        }

        /* string to double */
        private double str2dbl(string s)
        {
            try
            {
                string decimal_sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                string wrongSep = decimal_sep == "." ? "," : ".";
                return double.Parse(s.Replace(wrongSep, decimal_sep));
            }
            catch (Exception) { return 0; }
        }

        private void ChangeReferencePoint(object sender, RoutedEventArgs e)
        {
            var rp = (RadioButton)sender;
            switch (rp.Name)
            {
                case "rpTL": _rPoint = cdrReferencePoint.cdrTopLeft; break;
                case "rpTC": _rPoint = cdrReferencePoint.cdrTopMiddle; break;
                case "rpTR": _rPoint = cdrReferencePoint.cdrTopRight; break;
                case "rpML": _rPoint = cdrReferencePoint.cdrMiddleLeft; break;
                case "rpC": _rPoint = cdrReferencePoint.cdrCenter; break;
                case "rpMR": _rPoint = cdrReferencePoint.cdrMiddleRight; break;
                case "rpBL": _rPoint = cdrReferencePoint.cdrBottomLeft; break;
                case "rpBC": _rPoint = cdrReferencePoint.cdrBottomMiddle; break;
                case "rpBR": _rPoint = cdrReferencePoint.cdrBottomRight; break;
            }
        }

        private void TransformGetValue(object sender, RoutedEventArgs e)
        {
            if (dApp.Documents.Count == 0) return;
            if (dApp.ActiveSelectionRange.Count != 1) return;

            var shift = Keyboard.IsKeyDown(Key.LeftShift);
            var ctrl = Keyboard.IsKeyDown(Key.LeftCtrl);

            var doc = dApp.ActiveDocument;
            doc.Unit = doc.Rulers.HUnits;
            doc.ReferencePoint = _rPoint;

            try
            {
                boostStart(MName + ": Get values");
                var s = dApp.ActiveSelectionRange[1];

                var isRotate = false;
                double rX = 0;
                double rY = 0;

                var rAngle = s.RotationAngle;
                if (rAngle != 0)
                {
                    tbRotate.Text = Math.Round(rAngle, 2).ToString(CultureInfo.InvariantCulture);
                    isRotate = true;
                    rX = s.RotationCenterX;
                    rY = s.RotationCenterY;
                    s.Rotate(rAngle * -1);
                }
                else tbRotate.Text = "0";

                double sX, sY, sW, sH;
                s.GetBoundingBox(out sX, out sY, out sW, out sH, (bool)cbTransformUseOutline.IsChecked);

                //scale
                sW *= doc.WorldScale;
                sH *= doc.WorldScale;

                tbWidth.Text = !ctrl ? Math.Round(sW, 3).ToString(CultureInfo.InvariantCulture) : "0";
                tbHeight.Text = !shift ? Math.Round(sH, 3).ToString(CultureInfo.InvariantCulture) : "0";

                if (isRotate) s.RotateEx(rAngle, rX, rY);

                boostFinish(true);
            }
            catch (Exception err)
            {
                boostFinish(true);
                tbWidth.Text = "0";
                tbHeight.Text = "0";
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            doc.Undo();
        }

        private void TransformResize(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dApp.Documents.Count == 0) return;
                if (dApp.ActiveSelectionRange.Count == 0) return;

                boostStart(MName + ": Transformations");

                var doc = dApp.ActiveDocument;
                doc.Unit = doc.Rulers.HUnits;
                doc.ReferencePoint = _rPoint;

                var sr = dApp.ActiveSelectionRange;

                double rX = 0;
                double rY = 0;

                foreach (Shape s in sr)
                {
                    double rAngle = s.RotationAngle;
                    if (rAngle != 0)
                    {
                        rX = s.RotationCenterX;
                        rY = s.RotationCenterY;
                        s.Rotate(rAngle * -1);
                    }

                    DoResize(s);

                    if (rAngle != 0) s.RotateEx(rAngle, rX, rY);
                }

                boostFinish(true);
                sr.CreateSelection();

                SaveKeyVal("TransformUseOutline", BoolToVal((bool)cbTransformUseOutline.IsChecked));
            }
            catch (Exception err)
            {
                boostFinish(true);
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoResize(Shape s)
        {
            try
            {
                double width = str2dbl(tbWidth.Text), height = str2dbl(tbHeight.Text);
                if (width < 0) width = 0;
                if (height < 0) height = 0;

                width /= dApp.ActiveDocument.WorldScale;
                height /= dApp.ActiveDocument.WorldScale;

                if ((bool)cbTransformUseOutline.IsChecked && s.Outline.Type != cdrOutlineType.cdrNoOutline)
                {
                    double x, y, w, h; // без обводки
                    double xO, yO, wO, hO;

                    s.GetBoundingBox(out x, out y, out w, out h, false);
                    s.GetBoundingBox(out xO, out yO, out wO, out hO, true);

                    w *= dApp.ActiveDocument.WorldScale;
                    h *= dApp.ActiveDocument.WorldScale;
                    wO *= dApp.ActiveDocument.WorldScale;
                    hO *= dApp.ActiveDocument.WorldScale;

                    double newW = width, newH = height;

                    if (s.Outline.ScaleWithShape)
                    {
                        if (width > 0) newW = (w * (width * 100 / wO)) / 100;
                        if (height > 0) newH = (h * (height * 100 / hO)) / 100;
                    }
                    else
                    {
                        if (width > 0) newW = width - (wO - w);
                        if (height > 0) newH = height - (hO - h);
                    }

                    s.SetSize(newW, newH);
                }
                else s.SetSize(width, height);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TransformRotate(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dApp.Documents.Count == 0) return;
                if (dApp.ActiveSelectionRange.Count == 0) return;

                var ownRC = (bool)tbOwnRotationCenter.IsChecked;
                var isShift = Keyboard.IsKeyDown(Key.LeftShift);

                boostStart(MName + ": Rotation");

                var doc = dApp.ActiveDocument;
                doc.Unit = doc.Rulers.HUnits;
                doc.ReferencePoint = _rPoint;

                var sr = dApp.ActiveSelectionRange;

                foreach (Shape s in sr)
                {
                    if (ownRC)
                    {
                        if (isShift) s.Rotate(str2dbl(tbRotate.Text));
                        else s.Rotate(str2dbl(tbRotate.Text) - s.RotationAngle);
                    }
                    else
                    {
                        double rX, rY;
                        s.GetPositionEx(_rPoint, out rX, out rY);
                        s.SetRotationCenter(rX, rY);

                        if (isShift) s.Rotate(str2dbl(tbRotate.Text));
                        else s.Rotate(str2dbl(tbRotate.Text) - s.RotationAngle);
                    }
                }

                boostFinish(true);
                sr.CreateSelection();
            }
            catch (Exception err)
            {
                boostFinish(true);
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeRotationCenter(object sender, RoutedEventArgs e)
        {
            if ((bool)tbOwnRotationCenter.IsChecked) tbOwnRotationCenter.Content = "Own X";
            else tbOwnRotationCenter.Content = "User X";
        }

        private void ChangeProperty(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dApp.Documents.Count == 0) return;
                if (dApp.ActiveSelectionRange.Count == 0) return;

                var step = str2dbl(tbStep.Text);
                if (Keyboard.IsKeyDown(Key.LeftShift)) step *= 2;
                else if (Keyboard.IsKeyDown(Key.LeftCtrl)) step /= 2;

                var isTrue = true;

                var bt = (Button)sender;
                if (bt.Name == "bDecrease")
                {
                    step *= -1;
                    isTrue = false;
                }

                boostStart(MName + ": Change " + cbPropertyType.Text);

                var doc = dApp.ActiveDocument;
                doc.Unit = doc.Rulers.HUnits;
                doc.ReferencePoint = _rPoint;

                var sr = dApp.ActiveSelectionRange;

                ChangePropertyDo(sr, step, isTrue);

                boostFinish(true);
                sr.CreateSelection();

                SaveKeyVal("PropertyType", cbPropertyType.Text);

            }
            catch (Exception err)
            {
                boostFinish(true);
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePropertyDo(ShapeRange sr, double step, bool isTrue)
        {
            try
            {
                double dVal;
                float fVal;

                var isRotate = false;
                double rX = 0;
                double rY = 0;

                var itm = (ComboBoxItem)cbPropertyType.SelectedItem;

                foreach (Shape s in sr)
                {
                    if (s.Type == cdrShapeType.cdrGroupShape) ChangePropertyDo(s.Shapes.All(), step, isTrue);

                    switch (itm.Tag.ToString())
                    {
                        case "ctWidth":
                        case "ctHeight":
                            double rAngle = s.RotationAngle;
                            double sw = 0, sh = 0;
                            if (rAngle != 0)
                            {
                                isRotate = true;
                                rX = s.RotationCenterX;
                                rY = s.RotationCenterY;
                                s.Rotate(rAngle * -1);
                            }
                            s.GetSize(out sw, out sh);
                            if (itm.Tag.ToString() == "ctWidth") s.SetSize(sw + step, sh);
                            else s.SetSize(sw, sh + step);
                            if (isRotate) s.RotateEx(rAngle, rX, rY);
                            break;

                        case "ctOutlineWidth":
                            if (s.CanHaveOutline)
                            {
                                if (s.Outline.Type != cdrOutlineType.cdrNoOutline)
                                {
                                    dVal = s.Outline.Width + step;
                                    if (dVal <= 0) s.Outline.Width = 0.001;
                                    else s.Outline.Width = dVal;
                                }
                            }
                            break;

                        case "ctTextSize":
                            if (s.Type == cdrShapeType.cdrTextShape)
                            {
                                fVal = s.Text.Story.Size + (float)step;
                                if (fVal <= 0) s.Text.Story.Size = 1.0f;
                                s.Text.Story.Size = fVal;
                            }
                            break;

                        case "ctTextLineSp":
                            if (s.Type == cdrShapeType.cdrTextShape)
                            {
                                fVal = s.Text.Story.LineSpacing + (float)step;
                                if (fVal <= 0) s.Text.Story.LineSpacing = 1.0f;
                                s.Text.Story.LineSpacing = fVal;
                            }
                            break;

                        case "ctContourOffset":
                            if (s.Effects.ContourEffect != null)
                            {
                                var eff = s.Effects.ContourEffect.Contour;
                                dVal = eff.Offset + step;
                                if (dVal > 0) eff.Offset = dVal;
                            }
                            break;

                        case "ctContourType":
                            if (s.Effects.ContourEffect != null)
                            {
                                var eff = s.Effects.ContourEffect.Contour;
                                eff.Direction = isTrue ? cdrContourDirection.cdrContourOutside : cdrContourDirection.cdrContourInside;
                            }
                            break;

                    }
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoStepRepeat(object sender, RoutedEventArgs e)
        {
            if (dApp.Documents.Count == 0) return;
            if (dApp.ActiveSelectionRange.Count == 0) return;

            try
            {
                boostStart(MName + ": Step and Repeat");

                var hCount = Convert.ToInt32(Math.Round(str2dbl(tbHorizontalCount.Text)));
                var vCount = Convert.ToInt32(Math.Round(str2dbl(tbVerticalCount.Text)));

                var doc = dApp.ActiveDocument;
                doc.Unit = doc.Rulers.HUnits;

                var sr = dApp.ActiveSelectionRange;
                var srBak = dApp.ActiveSelectionRange;

                double x, y, h, w;
                sr.GetBoundingBox(out x, out y, out w, out h, (bool)cbStepRepeatUseOutline.IsChecked);

                double bSpace = str2dbl(tbStepRepeatSpace.Text) / doc.WorldScale; //with Scale

                var offsetX = w + bSpace;
                var offsetY = h + bSpace;

                var nSr = dApp.ActiveSelectionRange;

                for (int i = 1; i <= hCount; i++)
                {
                    sr = sr.Duplicate(offsetX, 0);
                    nSr = AddRange(nSr, sr);  //nSr.AddRange(sr); //AddRange не работает на ХР с обфускацией...
                }

                for (int i = 1; i <= vCount; i++) nSr = nSr.Duplicate(0, offsetY * -1);

                doc.ClearSelection();
                srBak.CreateSelection();

                boostFinish(true);

                SaveKeyVal("HorizontalCount", tbHorizontalCount.Text);
                SaveKeyVal("VerticalCount", tbVerticalCount.Text);
                SaveKeyVal("StepRepeatSpace", tbStepRepeatSpace.Text);
                SaveKeyVal("StepRepeatUseOutline", BoolToVal((bool)cbStepRepeatUseOutline.IsChecked));
            }
            catch (Exception err)
            {
                boostFinish(true);
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ShapeRange AddRange(ShapeRange nSr, ShapeRange sr)
        {
            try
            {
                foreach (Shape s in sr) nSr.Add(s);
                return nSr;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
                return nSr;
            }
        }

        private void Distribute(object sender, RoutedEventArgs e)
        {
            if (dApp.Documents.Count == 0) return;
            if (dApp.ActiveSelectionRange.Count < 2) return;

            try
            {
                boostStart(MName + ": Distribute");

                var doc = dApp.ActiveDocument;
                doc.Unit = doc.Rulers.HUnits;
                doc.ReferencePoint = cdrReferencePoint.cdrCenter;

                double space = str2dbl(tbDistributeSpace.Text) / doc.WorldScale; //with Scale
                var sr = dApp.ActiveSelectionRange;
                double centerX, centerY;
                sr.GetPosition(out centerX, out centerY);

                var bt = (Button)sender;
                switch (bt.Name)
                {
                    case "dhb": case "dl": case "dhc": sr.Sort("@shape1.Left < @shape2.Left"); break;
                    case "dr": sr.Sort("@shape1.Left > @shape2.Left"); break;
                    case "dvb": case "dt": case "dvc": sr.Sort("@shape1.Top > @shape2.Top"); break;
                    case "db": sr.Sort("@shape1.Top < @shape2.Top"); break;
                }

                for (int i = 2; i <= sr.Count; i++)
                {
                    double left, bottom, w, h, tleft, tbottom, tw, th;
                    sr[i - 1].GetBoundingBox(out left, out bottom, out w, out h, (bool)cbDistributeUseOutline.IsChecked);
                    sr[i].GetBoundingBox(out tleft, out tbottom, out tw, out th, (bool)cbDistributeUseOutline.IsChecked);

                    switch (bt.Name)
                    {
                        case "dhb": sr[i].CenterX = left + w + space + (tw / 2); break;
                        case "dl": sr[i].CenterX = left + space + (tw / 2); break;
                        case "dhc": sr[i].CenterX = left + (w / 2) + space; break;
                        case "dr": sr[i].CenterX = (left + w) - space - (tw / 2); break;
                        case "dvb": sr[i].CenterY = bottom - space - (th / 2); break;
                        case "dt": sr[i].CenterY = bottom + h - space - (th / 2); break;
                        case "dvc": sr[i].CenterY = bottom + (h / 2) - space; break;
                        case "db": sr[i].CenterY = bottom + space + (th / 2); break;
                    }
                }

                switch (bt.Name)
                {
                    case "dhb": case "dhc": case "dvb": case "dvc": sr.SetPosition(centerX, centerY); break;
                }

                doc.ClearSelection();
                sr.CreateSelection();

                boostFinish(true);

                SaveKeyVal("DistributeSpace", tbDistributeSpace.Text);
                SaveKeyVal("DistributeUseOutline", BoolToVal((bool)cbDistributeUseOutline.IsChecked));
            }
            catch (Exception err)
            {
                boostFinish(true);
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MakeCropMarks(object sender, RoutedEventArgs e)
        {
            if (dApp.Documents.Count == 0) return;
            if (dApp.ActiveSelectionRange.Count == 0) return;

            try
            {
                boostStart(MName + ": Make Crop Marks");

                var doc = dApp.ActiveDocument;
                doc.Unit = doc.Rulers.HUnits;
                doc.ReferencePoint = cdrReferencePoint.cdrCenter;

                var sr = dApp.ActiveSelectionRange;

                if (!(bool)cbCropMarksRange.IsChecked) DoMakeCropMarks(sr);
                else foreach (Shape s in sr) DoMakeCropMarks(null, s);

                doc.ClearSelection();
                sr.CreateSelection();

                boostFinish(true);

                SaveKeyVal("CropMarksLenght", tbCropMarksLenght.Text);
                SaveKeyVal("CropMarksOffset", tbCropMarksOffset.Text);
                SaveKeyVal("CropMarksStrokeWeight", tbCropMarksStrokeWeight.Text);
                SaveKeyVal("CropMarksUseOutline", BoolToVal((bool)cbCropMarksUseOutline.IsChecked));
                SaveKeyVal("CropMarksEachObject", BoolToVal((bool)cbCropMarksRange.IsChecked));
            }
            catch (Exception err)
            {
                boostFinish(true);
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoMakeCropMarks(ShapeRange sr = null, Shape sh = null)
        {
            try
            {
                if (sr == null && sh == null) return;

                double lenght = str2dbl(tbCropMarksLenght.Text) / dApp.ActiveDocument.WorldScale; //with Scale
                double offset = str2dbl(tbCropMarksOffset.Text) / dApp.ActiveDocument.WorldScale; //with Scale
                double weight = str2dbl(tbCropMarksStrokeWeight.Text) / dApp.ActiveDocument.WorldScale; //with Scale

                double left, bottom, w, h;
                if (sr != null) sr.GetBoundingBox(out left, out bottom, out w, out h, (bool)cbCropMarksUseOutline.IsChecked);
                else sh.GetBoundingBox(out left, out bottom, out w, out h, (bool)cbCropMarksUseOutline.IsChecked);

                double right = left + w;
                double top = bottom + h;
                double centerX = left + (w / 2);
                double centerY = bottom + (h / 2);

                var l = dApp.ActiveLayer;

                Shape s;

                if ((bool)cmLT.IsChecked) { s = l.CreateLineSegment(left - lenght - offset, top, left - offset, top); SetOutlineStyle(s, weight); }
                if ((bool)cmTL.IsChecked) { s = l.CreateLineSegment(left, top + lenght + offset, left, top + offset); SetOutlineStyle(s, weight); }
                if ((bool)cmTC.IsChecked) { s = l.CreateLineSegment(centerX, top + lenght + offset, centerX, top + offset); SetOutlineStyle(s, weight); }
                if ((bool)cmTR.IsChecked) { s = l.CreateLineSegment(right, top + lenght + offset, right, top + offset); SetOutlineStyle(s, weight); }
                if ((bool)cmRT.IsChecked) { s = l.CreateLineSegment(right + lenght + offset, top, right + offset, top); SetOutlineStyle(s, weight); }

                if ((bool)cmCL.IsChecked) { s = l.CreateLineSegment(left - lenght - offset, centerY, left - offset, centerY); SetOutlineStyle(s, weight); }
                if ((bool)cmCR.IsChecked) { s = l.CreateLineSegment(right + lenght + offset, centerY, right + offset, centerY); SetOutlineStyle(s, weight); }

                if ((bool)cmLB.IsChecked) { s = l.CreateLineSegment(left - lenght - offset, bottom, left - offset, bottom); SetOutlineStyle(s, weight); }
                if ((bool)cmBL.IsChecked) { s = l.CreateLineSegment(left, bottom - lenght - offset, left, bottom - offset); SetOutlineStyle(s, weight); }
                if ((bool)cmBC.IsChecked) { s = l.CreateLineSegment(centerX, bottom - lenght - offset, centerX, bottom - offset); SetOutlineStyle(s, weight); }
                if ((bool)cmBR.IsChecked) { s = l.CreateLineSegment(right, bottom - lenght - offset, right, bottom - offset); SetOutlineStyle(s, weight); }
                if ((bool)cmRB.IsChecked) { s = l.CreateLineSegment(right + lenght + offset, bottom, right + offset, bottom); SetOutlineStyle(s, weight); }
            }
            catch (Exception err) { MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void SetOutlineStyle(Shape s, double weight)
        {
            s.Fill.ApplyNoFill();
            s.Outline.SetProperties(weight, null,
                _marksColor, null, null,
                cdrTriState.cdrFalse,
                cdrTriState.cdrFalse,
                cdrOutlineLineCaps.cdrOutlineButtLineCaps,
                cdrOutlineLineJoin.cdrOutlineMiterLineJoin);
        }

        private void CheckAllMarks(object sender, RoutedEventArgs e) { AllMarks(true); }
        private void UncheckAllMarks(object sender, RoutedEventArgs e) { AllMarks(false); }

        private void AllMarks(Boolean state)
        {
            cmTL.IsChecked = state;
            cmLT.IsChecked = state;
            cmTC.IsChecked = state;
            cmTR.IsChecked = state;
            cmRT.IsChecked = state;

            cmCL.IsChecked = state;
            cmCR.IsChecked = state;

            cmBL.IsChecked = state;
            cmLB.IsChecked = state;
            cmBC.IsChecked = state;
            cmBR.IsChecked = state;
            cmRB.IsChecked = state;
        }

        private void DefaultMarks(object sender, RoutedEventArgs e) { DefMarksApply(); }
        private void DefMarksApply()
        {
            AllMarks(true);
            cmTC.IsChecked = false;
            cmCL.IsChecked = false;
            cmCR.IsChecked = false;
            cmBC.IsChecked = false;
        }

        private void ChangeMarksColor(object sender, RoutedEventArgs e)
        {
            if (!_marksColor.UserAssignEx()) return;
            ApplyColorUI();
            SaveKeyVal("CropMarksColor", _marksColor.ToString());
        }

        private void CbOffsetClick(object sender, RoutedEventArgs e)
        {
            if (dApp.Documents.Count == 0) return;
            if (dApp.ActiveSelectionRange.Count == 0) return;

            try
            {
                var offset = str2dbl(tbOffset.Text) / dApp.ActiveDocument.WorldScale; //with Scale
                if (offset == 0) return;

                boostStart(MName + ": Offset Path");

                var doc = dApp.ActiveDocument;
                doc.Unit = doc.Rulers.HUnits;

                var direction = cdrContourDirection.cdrContourOutside;

                if (offset < 0)
                {
                    direction = cdrContourDirection.cdrContourInside;
                    offset *= -1;
                }

                var itm = (ComboBoxItem)cbCornerType.SelectedItem;

                cdrContourCornerType cType;
                cdrContourEndCapType ceType;
                switch (itm.Tag.ToString())
                {
                    case "Mitered":
                        cType = cdrContourCornerType.cdrContourCornerMiteredOffsetBevel;
                        ceType = cdrContourEndCapType.cdrContourSquareCap;
                        break;
                    case "Round":
                        cType = cdrContourCornerType.cdrContourCornerRound;
                        ceType = cdrContourEndCapType.cdrContourRoundCap;
                        break;
                    default:
                        cType = cdrContourCornerType.cdrContourCornerBevel;
                        ceType = cdrContourEndCapType.cdrContourSquareCap;
                        break;
                }

                var sr = dApp.ActiveSelectionRange;

                foreach (Shape s in sr)
                {
                    switch (s.Type.ToString())
                    {
                        case "cdrCurveShape":
                        case "cdrEllipseShape":
                        case "cdrTextShape":
                        case "cdrPolygonShape":
                        case "cdrPerfectShape":
                        case "cdrRectangleShape":
                            if (s.Effects.ContourEffect != null) s.Effects.ContourEffect.Clear();
                            var eff = s.CreateContour(direction, offset, 1, cdrFountainFillBlendType.cdrDirectFountainFillBlend,
                                            null, null, null, 0, 0, ceType,
                                            cType, str2dbl(tbMiterLimit.Text));
                            var sh = eff.Separate().Shapes[1];
                            int prop = cdrCopyProperties.cdrCopyFill.GetHashCode() +
                                       cdrCopyProperties.cdrCopyOutlinePen.GetHashCode() +
                                       cdrCopyProperties.cdrCopyOutlineColor.GetHashCode();
                            sh.CopyPropertiesFrom(s, (cdrCopyProperties)prop);
                            if ((bool)cbInvertColor.IsChecked) sh.ApplyEffectInvert();
                            break;
                    }
                }

                doc.ClearSelection();
                sr.CreateSelection();

                boostFinish(true);

                SaveKeyVal("CornerType", cbCornerType.Text);
                SaveKeyVal("Offset", tbOffset.Text);
                SaveKeyVal("MiterLimit", tbMiterLimit.Text);
                SaveKeyVal("InvertColor", BoolToVal((bool)cbInvertColor.IsChecked));

            }
            catch (Exception err)
            {
                boostFinish(true);
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btOffsetChange(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var step = str2dbl(tbStep.Text);
            if (btn.Tag.ToString() == "down") step *= -1;
            switch (btn.Name)
            {
                case "btWidthUp":
                case "btWidthDown":
                    tbWidth.Text = (str2dbl(tbWidth.Text) + step).ToString(CultureInfo.InvariantCulture);
                    break;
                case "btHeightUp":
                case "btHeightDown":
                    tbHeight.Text = (str2dbl(tbHeight.Text) + step).ToString(CultureInfo.InvariantCulture);
                    break;

                case "btHorizontalCountUp":
                case "btHorizontalCountDown":
                    tbHorizontalCount.Text = (str2dbl(tbHorizontalCount.Text) + Math.Round(step)).ToString(CultureInfo.InvariantCulture);
                    break;
                case "btVerticalCountUp":
                case "btVerticalCountDown":
                    tbVerticalCount.Text = (str2dbl(tbVerticalCount.Text) + Math.Round(step)).ToString(CultureInfo.InvariantCulture);
                    break;

                case "btStepRepeatSpaceUp":
                case "btStepRepeatSpaceDown":
                    tbStepRepeatSpace.Text = (str2dbl(tbStepRepeatSpace.Text) + step).ToString(CultureInfo.InvariantCulture);
                    break;

                case "btDistributeSpaceUp":
                case "btDistributeSpaceDown":
                    tbDistributeSpace.Text = (str2dbl(tbDistributeSpace.Text) + step).ToString(CultureInfo.InvariantCulture);
                    break;

                case "btOffsetUp":
                case "btOffsetDown":
                    tbOffset.Text = (str2dbl(tbOffset.Text) + step).ToString(CultureInfo.InvariantCulture);
                    break;
                case "btMiterUp":
                case "btMiterDown":
                    tbMiterLimit.Text = (str2dbl(tbMiterLimit.Text) + step).ToString(CultureInfo.InvariantCulture);
                    break;
            }
        }

        private void ReplaceText(object sender, RoutedEventArgs e)
        {
            if (dApp.Documents.Count == 0) return;
            if (dApp.ActiveSelectionRange.Count == 0) return;

            try
            {
                boostStart(MName + ": replace text");

                string pattern = tbFind.Text;
                string replace = FixReplaceTxt(tbReplace.Text);

                foreach (Shape s in dApp.ActiveSelectionRange)
                {
                    if (s.Type != cdrShapeType.cdrTextShape) continue;

                    string txt = FixFindTxt(s.Text.Story.WideText);
                    var regex = new Regex(pattern);

                    MatchCollection mc = regex.Matches(txt);
                    if (mc.Count == 0) continue;

                    for (int i = mc.Count - 1; i > -1; i--)
                    {
                        Match m = mc[i];
                        TextRange tr = s.Text.Story.Range(m.Index, (m.Index + m.Length));

                        if (_findStyle != null)
                        {
                            string style = tr.ObjectStyle.ToString();
                            if (style.IndexOf(_findStyle.Name, StringComparison.Ordinal) == -1) continue;
                        }

                        string newTxt = regex.Replace(FixFindTxt(tr.WideText), replace);
                        tr.WideText = newTxt;

                        if (_replaceStyle != null) tr.ApplyStyle(_replaceStyle.Name);

                    }
                }

                boostFinish(true);
            }
            catch (Exception err)
            {
                boostFinish(true);
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FixFindTxt(string txt)
        {
            const char cdrN = '\u000B';
            const char trueN = '\u000A';
            return txt.Replace(cdrN, trueN);
        }

        private string FixReplaceTxt(string txt)
        {
            const char n = '\u000A';
            const char r = '\u000D';
            const char t = '\u0009';
            txt = txt.Replace(@"\n", n.ToString(CultureInfo.InvariantCulture));
            txt = txt.Replace(@"\r", r.ToString(CultureInfo.InvariantCulture));
            txt = txt.Replace(@"\t", t.ToString(CultureInfo.InvariantCulture));
            return txt;
        }

        private void ChooseStyle(object sender, RoutedEventArgs e)
        {
            var bt = (Button)sender;
            if (bt.Name == "bFindStyle")
            {
                UpdateStylesLists(FindStyles);
                FindStyles.PlacementTarget = this;
                FindStyles.IsOpen = true;
            }
            else
            {
                UpdateStylesLists(ReplaceStyles);
                ReplaceStyles.PlacementTarget = this;
                ReplaceStyles.IsOpen = true;
            }
        }

        private void UpdateStylesLists(ContextMenu menu)
        {
            if (dApp.Documents.Count == 0) return;
            var doc = dApp.ActiveDocument;
            menu.Items.Clear();

            var mi = new MenuItem { Header = "None", IsCheckable = true };
            mi.Click += new RoutedEventHandler(SetStyle);
            if (menu.Name == "FindStyles" && _findStyle == null) mi.IsChecked = true;
            if (menu.Name == "ReplaceStyles" && _replaceStyle == null) mi.IsChecked = true;
            menu.Items.Add(mi);

            foreach (Style st in doc.StyleSheet.AllStyles)
            {
                mi = new MenuItem { Header = st.Name, IsCheckable = true };
                mi.Click += new RoutedEventHandler(SetStyle);
                if (menu.Name == "FindStyles" && _findStyle != null && _findStyle.Name == st.Name) mi.IsChecked = true;
                if (menu.Name == "ReplaceStyles" && _replaceStyle != null && _replaceStyle.Name == st.Name) mi.IsChecked = true;
                menu.Items.Add(mi);
            }
            foreach (Style st in doc.StyleSheet.AllStyleSets)
            {
                mi = new MenuItem { Header = st.Name, IsCheckable = true };
                mi.Click += new RoutedEventHandler(SetStyle);
                if (menu.Name == "FindStyles" && _findStyle != null && _findStyle.Name == st.Name) mi.IsChecked = true;
                if (menu.Name == "ReplaceStyles" && _replaceStyle != null && _replaceStyle.Name == st.Name) mi.IsChecked = true;
                menu.Items.Add(mi);
            }
        }

        private void SetStyle(object sender, RoutedEventArgs e)
        {
            if (dApp.Documents.Count == 0) return;
            var doc = dApp.ActiveDocument;

            var mi = (MenuItem)sender;
            mi.IsChecked = true;

            var menu = (ContextMenu)mi.Parent;
            if (menu.Name == "FindStyles")
            {
                _findStyle = mi.Header.ToString() == "None" ? null : doc.StyleSheet.FindStyle(mi.Header.ToString());
            }
            else
            {
                _replaceStyle = mi.Header.ToString() == "None" ? null : doc.StyleSheet.FindStyle(mi.Header.ToString());
            }
        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dApp.Documents.Count == 0) return;
                var sr = dApp.ActiveSelectionRange;
                dApp.ActiveDocument.Undo();
                if (sr != null)
                {
                    dApp.ActiveDocument.ClearSelection();
                    sr.CreateSelection();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            var w = new wAbout();
            var wih = new System.Windows.Interop.WindowInteropHelper(w) { Owner = (IntPtr)dApp.AppWindow.Handle };
            w.ShowDialog();
        }

        private void CheckUpdates(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = new WebClient();
                var source = x.DownloadString(MWebPage);
                var title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\s\|.+\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;

                if ((MName + " " + MVer) != title)
                {
                    var ans = MessageBox.Show(title + " available.\nGo to web page for download new version?", MName, MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (ans.GetHashCode() == 1) System.Diagnostics.Process.Start(MWebPage);
                }
                else MessageBox.Show("There are no updates available at this time.", MName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
