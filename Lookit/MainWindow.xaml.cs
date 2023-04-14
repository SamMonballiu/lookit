using Lookit.Context;
using Lookit.Helpers;
using Lookit.Logic;
using Lookit.Models;
using Lookit.ViewModels;
using Lookit.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;


namespace Lookit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // at class level
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public LookitMainViewModel Viewmodel => (DataContext as LookitMainViewModel);

        private readonly OpenFileDialog _loadFileDialog = new()
        {
            Filter = "JSON files|*.json"
        };

        private readonly SaveFileDialog _saveFileDialog = new()
        {
            Filter = "JSON files|*.json"
        };

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new LookitMainViewModel();
            zoomPicker.ZoomChanged += ZoomPicker_ZoomChanged;

            ListMeasurements.SelectionChanged += (s, e) =>
            {
                try
                {
                    Viewmodel.SelectedMeasurement = Viewmodel.Measurements[ListMeasurements.SelectedIndex]?.Measurement;
                } catch
                {
                    //TODO Fix
                }
            };
        }

        private async void DdnPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageContext.SelectPage((int)ddnPages.SelectedValue);
            Viewmodel.SelectedPage = PageContext.SelectedPage;
            Viewmodel.OnSetImageSource.Execute(PageContext.Get(PageContext.SelectedPage));
            var pageNumber = PageContext.SelectedPage;
            if (PageContext.Has(pageNumber))
            {
                Viewmodel.OnSetImageSource.Execute(PageContext.Get(pageNumber));
                return;
            }

            Viewmodel.OnSetImageSource.Execute(PDF.GetImage(pageNumber));
        }

        public void ShowPdfPicker()
        {
            new PdfPicker().ShowDialog();
            Viewmodel?.OnSetImageSource.Execute(PageContext.Get(PageContext.SelectedPage));
            UpdatePagesDropdown();
        }

        private void ZoomPicker_ZoomChanged(object sender, UserControls.ZoomChangedEventArgs e)
        {
            (DataContext as LookitMainViewModel).ZoomLevel = e.Value;
        }

        private void ImgControl_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (Viewmodel.Mode is Mode.None)
                    {
                        return;
                    }
                    if (Viewmodel.Mode is Mode.Scale)
                    {
                        var pos = e.GetPosition(ImgMain);

                        Viewmodel.OnAddPoint.Execute(pos.ToPoint());

                        if (Viewmodel.TempPoints.Count == 2)
                        {
                            var view = new SetScaleView(Viewmodel.LinePreview.Start, Viewmodel.LinePreview.End);
                            view.OnConfirm += (Scale scale) => Viewmodel.OnSetScale?.Execute(scale);
                            view.ShowDialog();
                        }
                        return;
                    }

                    Viewmodel.OnAddPoint.Execute(e.GetPosition(ImgMain).ToPoint());
                    break;

                case MouseButton.Right:
                    Viewmodel.OnRemovePoint.Execute(null);
                    break;
            }
        }

        private void BtnScale_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.Scale;
        }

        private void BtnMeasure_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.MeasurePolygon; //TODO!
        }

        private void BtnMeasureLine_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.MeasureLine;
        }


        private void BtnMeasureRect_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.MeasureRectangle;
        }

        private void BtnUpdateScale_Click(object sender, RoutedEventArgs e)
        {
            ScaleContext.Scale = ScaleContext.Scale.UpdateDistance(double.Parse(TxtScaleDistance.Text));
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            ShowPdfPicker();
        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!LookitMainViewModel.IsControlDown())
            {
                return;
            }

            if (e.Delta > 0)
            {
                zoomPicker.ZoomIn();
            } 
            else
            {
                zoomPicker.ZoomOut();
            }
        }

        private async void btnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_loadFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    await LoadPersistedSession(_loadFileDialog.FileName);
                
                    UpdatePagesDropdown();
                }
            } catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (_saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PersistToSession(_saveFileDialog.FileName);
            }
        }

        private void UpdatePagesDropdown()
        {
            if (PageContext.PDF != null)
            {
                var pages = Enumerable.Range(1, (int)PageContext.PDF.PageCount).ToList();
                ddnPages.ItemsSource = pages;
                ddnPages.SelectionChanged += DdnPages_SelectionChanged;
                ddnPages.SelectedValue = PageContext.SelectedPage;
            }
        }

        public async Task LoadPersistedSession(string filename)
        {
            var persistedSession = Persist.ReadSession(filename);
            this.DataContext = await LookitMainViewModel.FromPersistedSession(persistedSession);
            PageContext.SelectPage(persistedSession.SelectedPage);
            PageContext.Filename = persistedSession.PdfPath;
        }

        public void PersistToSession(string filename)
        {
            var persistableSession = Viewmodel.GetPersistableSession();
            Persist.PersistSession(persistableSession, filename);
        }

        private void ScrollViewer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(ImgMain);
            if ((Viewmodel.Mode is Mode.MeasureLine or Mode.Scale) && Viewmodel.TempPoints.Count > 0)
            {
                Viewmodel.OnUpdateTemporaryPoint.Execute(pos.ToPoint());
            }
        }
    }
}
