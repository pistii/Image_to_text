using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Word;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Tesseract;
using OpenCvSharp;
using System.Reflection.Metadata;
using Document = Microsoft.Office.Interop.Word.Document;

namespace Image_to_text
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        readonly ItemType itemType;

        string importPath;
        string fileName = "KepbolSzoveg";
        OpenFileDialog ofd;
        string desktopPath;
        public MainWindow()
        {
            InitializeComponent();

            itemType = new ItemType();
            DataContext = itemType;
            desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            exportPath.Text = Path.Combine(desktopPath + @"\");

            AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(AppDomain_UnhandledException);

        }

        public event System.Windows.Threading.DispatcherUnhandledExceptionEventHandler DispatcherUnhandledException;
        public ObservableCollection<ItemType> exportTypes = new ObservableCollection<ItemType>();


        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            LoadImage();
        }

        private void LoadImage()
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (JPG,JPEG,PNG,TIFF)|*.JPG;*.JPEG;*.PNG;*.TIFF";

            if (ofd.ShowDialog() == true)
            {
                importPath = ofd.FileName;
                FileSource.Text = ofd.FileName;
            }
        }

        private void btnConvert(object sender, RoutedEventArgs e)
        {
            ConvertImage(itemType.SelectedItemType.Name);
        }


        private void ConvertImage(string exportType)
        {
            ///<summary>
            /// export path
            /// </summary>
            string fullpath = string.Empty;
            string text;
            string tesseractPath = string.Empty;
            //Create log file to improve error handling and process management
            using (StreamWriter wr = new StreamWriter(@"log.txt"))
            {
                try
                {
                    tesseractPath = @".\tessdata"; //Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) + @"tesseract");
                    wr.WriteLine("started\ntesseractPath: " + tesseractPath);

                    if (exportType == "Txt")
                        fullpath = Path.Combine(exportPath.Text, fileName + ".txt");
                    if (exportType == "Pdf")
                        fullpath = Path.Combine(exportPath.Text, fileName + ".pdf");
                    if (ofd.CheckFileExists)
                    {
                        wr.WriteLine("ExportPath: " + fullpath);
                        var lang = itemType.SelectedTranslateType.Language;
                        Mat image = Cv2.ImRead(importPath.ToString(), ImreadModes.Grayscale);
                        Mat binaryImage = new Mat();
                        Cv2.AdaptiveThreshold(image, binaryImage, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);
                        Cv2.GaussianBlur(image, binaryImage, new OpenCvSharp.Size(5, 5), 0);
                        Cv2.BitwiseNot(image, binaryImage);
                        wr.WriteLine("Img read, binarize and image processed...");
                        string dirpath = System.IO.Directory.GetParent(@"..\.").FullName;
                        wr.WriteLine("image save: " + dirpath);
                        string savedImagePath = Path.Combine(dirpath.ToString(), "img.png");
                        wr.WriteLine("image save path: " + savedImagePath);
                        binaryImage.SaveImage(savedImagePath);
                        wr.WriteLine("image saved: ");
                        wr.WriteLine("tesseract engine starting...");
                        wr.WriteLine("tesseract path: " + tesseractPath);

                        using (var engine = new TesseractEngine(tesseractPath, lang.ToLower().ToString(), EngineMode.Default))
                        {
                            engine.SetVariable("user_defined_dpi", "1000"); //set dpi for supressing warning
                            wr.WriteLine("tesseract engine running...");

                            using (var img = Pix.LoadFromFile(savedImagePath))
                            {
                                wr.WriteLine("image load...");
                                img.Deskew();

                                using (var page = engine.Process(img))
                                {
                                    wr.WriteLine("image processing...");

                                    text = page.GetText();
                                    //MessageBox.Show(text);
                                }
                            }
                            //MessageBox.Show(Path.Combine(System.IO.Directory.GetParent(@"..\.").FullName));
                            wr.WriteLine("try to delete image...");
                            try
                            {
                                File.Delete(savedImagePath);
                                wr.WriteLine("image deleted.");
                            }
                            catch (Exception e)
                            {
                                wr.WriteLine("Error: " + e.Message);
                            }

                        }
                        if (exportType == "Word")
                            CreateWordDoc(text);
                        else if (exportType == "Pdf")
                        {
                            try
                            {
                                var d = File.Create(fullpath);
                                var byteArray = ASCIIEncoding.ASCII.GetBytes(text);
                                d.Write(byteArray, 0, byteArray.Length);
                                MessageBox.Show("Document created successfully!");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error while creating document" + ex);
                                wr.WriteLine("Error while creating document: " + ex.Message + "\nDocType: pdf");
                            }
                        }
                        else if (exportType == "Txt")
                        {
                            try
                            {
                                using (StreamWriter sr = new StreamWriter(fullpath))
                                {
                                    sr.WriteLine(text.ToString());
                                }
                                MessageBox.Show("Document created successfully!");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error while creating document" + ex);
                                wr.WriteLine("Error while creating document: " + ex.Message + "\nDocType: txt");

                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("File path is not correct");
                        wr.WriteLine("Wrong file path...");

                    }
                }
                catch (NullReferenceException e)
                {
                    MessageBox.Show("There's no file selected: " + e.Message);
                }
            }
        }

        private void CreateWordDoc(string text)
        {
            try
            {
                var app = new Word.Application();

                object missing = Missing.Value;
                Document document = app.Documents.Add(ref missing, ref missing, ref missing, ref missing);

                document.Content.SetRange(0, 0);
                document.Content.Text = text + Environment.NewLine;

                object filename = exportPath.Text + fileName;
                document.SaveAs2(ref filename);
                document.Close(ref missing, ref missing, ref missing);
                document = null;
                app.Quit(ref missing, ref missing, ref missing);
                app = null;
                MessageBox.Show("Document created successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while creating document" + ex);
            }
        }


        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            using (StreamWriter sr = new StreamWriter(desktopPath + @"\error.txt"))
            {
                sr.WriteLine(e.Exception.Message);
            }
            MessageBox.Show("Error file written" + e.Exception);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

        /// <summary>
        /// Application domain exception handler
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public static void AppDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            using (StreamWriter sr = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\error.txt"))
            {
                sr.WriteLine(e.ExceptionObject);
            }
            MessageBox.Show("Error: " + e.ExceptionObject);
        }
    }
}
