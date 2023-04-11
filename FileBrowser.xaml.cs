using System;
using System . IO;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;


namespace GetFilesBySelection
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : Window
    {
        public static bool IsDirty { get; set; } = false;
        private string filename { get; set; } = "";
        private string splitype { get; set; }
        private int linecount { get; set; } = 0;
        private bool haslinenumbers { get; set; } = false;
        private string rootPath { get; set; } = "";

        public FileBrowser Filebrowser { get; set; }
        public SearchWin Searchwin { get; set; }
        public MainWindow Mainwindow { get; set; }

        public FileBrowser ( MainWindow Mainwindow, string sourcefile )
        {
            InitializeComponent ( );
            filename = sourcefile;
            LoadFile (filename );
            SetupWindowDrag ( this );
            this.Filebrowser = this;
            this . Mainwindow = Mainwindow;
        }

        private void LoadFile (string fileName )
        {
            if ( fileName != filename )
                filename = fileName;
            Sourcefile . Text = File . ReadAllText ( filename );
            if ( Sourcefile . Text . Contains ( "\r\n" ) == true )
            {
                splitype = "\r\n";
                string [ ] t = Sourcefile . Text . Split ( "\r\n" );
                linecount = t . Length;
            }
            else if ( Sourcefile . Text . Contains ( "\n" ) == true )
            {
                splitype = "\n";
                string [ ] t = Sourcefile . Text . Split ( "\n" );
                linecount = t . Length;
            }
            Sourcefile . Text = TabsToSpaces ( Sourcefile . Text , 4 );
            infopanel1 . Text = $"{filename}";
            infopanel2 . Text = $"{linecount} lines totaling {Sourcefile . Text . Length} bytes";
            IsDirty = false;
        }
private void CloseBtn_Click ( object sender , RoutedEventArgs e )
        {
            if ( IsDirty )
            {
                MessageBoxResult mbr = MessageBox . Show ( "Changes appear to have been made to this file ? \n\nDo you want them saved ?" , "Flle has been changed !" , MessageBoxButton . YesNo , MessageBoxImage . Question );
                if ( mbr == MessageBoxResult . Yes )
                {
                    if ( haslinenumbers )
                        StripLineNumbers ( );
                    File . WriteAllText ( filename , Sourcefile . Text );
                }
            }
            this . Close ( );
            if ( Mainwindow . Searchwin != null )
                Mainwindow . Searchwin . Close ( );
        }

        private void Dolineno_Click ( object sender , RoutedEventArgs e )
        {
            Mouse . OverrideCursor = Cursors . Wait;

            if ( DoLines . IsChecked == true )
            {
                AddLineNumbers ();
                Sourcefile . UpdateLayout ( );
                DoLines . Background = FindResource ( "Red6" ) as SolidColorBrush;
                DoLines . Foreground = FindResource ( "Red6" ) as SolidColorBrush;
            }
            else
            {
                StripLineNumbers ( );
            }
            Mouse . OverrideCursor = Cursors . Arrow;
        }
        private void AddLineNumbers ()
        {
            Mouse . OverrideCursor = Cursors . Wait;
            string str = Sourcefile . Text;
            Sourcefile . Text = "";
            string [ ] data = str.Split(splitype );
            for ( int x = 0 ; x < data . Length ; x++ )
            {
                if ( x < 10 )
                    data [ x ] = $"    {x} : {data [ x ]}{splitype}";
                else if ( x < 100 )
                    data [ x ] = $"   {x} : {data [ x ]}{splitype}";
                else if ( x < 1000 )
                    data [ x ] = $"  {x} : {data [ x ]}{splitype}";
                else if ( x < 10000 )
                    data [ x ] = $" {x} : {data [ x ]}{splitype}";
                else
                    data [ x ] = $"{x} : {data [ x ]}{splitype}";

                Sourcefile . Text += data [ x ];
            }
            haslinenumbers = true;
            Mouse . OverrideCursor = Cursors . Arrow;
            IsDirty = false;
        }
        private void StripLineNumbers ( )
        {
            File . WriteAllText ( filename + ".BAK" , Sourcefile . Text );
            string [ ] data = File . ReadAllLines ( filename + ".BAK" );
            File . Delete ( filename + ".BAK" );
            Sourcefile . Text = "";
            for ( int x = 0 ; x < data . Length ; x++ )
            {
                // strip line # from file
                Sourcefile . Text += data [ x ] . Substring ( 8 ) + splitype;
            }
            Sourcefile . UpdateLayout ( );
            DoLines . Background = FindResource ( "Green8" ) as SolidColorBrush;
            DoLines . Foreground = FindResource ( "Green8" ) as SolidColorBrush;
            haslinenumbers = false;
            IsDirty = false;
        }
        private string TabsToSpaces ( string input , int spacesTouse  )
        {
            // replace tabs, (or multiple spaces) and replace with specified # of spaces
            string output = "";
            string spacestring = "                    " . Substring ( 0 , spacesTouse );
            string [ ] lines = new string [ 1 ];
            string  parts;
            string tmp = "", tmp2 = "";
            Mouse . OverrideCursor = Cursors . Wait;

            if ( input.Contains("\t") )
            {
                // got  tabs
                lines = input . Split ( splitype );
                for ( int x = 0 ; x < lines . Length ; x++ )
                {
                    parts = lines [ x ];//. Split ( "\t" );
                    tmp = "";
                    for ( int y = 0 ; y < parts . Length ; y++ )
                    {
                        if ((char) parts [ y ] == '\t' )
                            tmp += $"{spacestring}";
                        else
                            tmp += parts [ y ];
                    }
                    if(tmp.EndsWith( splitype ) == false )
                        output += $"{tmp . ToString ( )}{splitype}";
                    else
                        output += $"{tmp . ToString ( )}";
                }
            }
            else 
                output = input;
            Mouse . OverrideCursor = Cursors . Arrow;
            return output;
        }

        private void Wrap_Click ( object sender , RoutedEventArgs e )
        {
            if ( WrapLines . IsChecked == false )
                Sourcefile . TextWrapping = TextWrapping . NoWrap;
            else
                Sourcefile . TextWrapping = TextWrapping . Wrap;
        }

        private void Sourcefile_TextChanged ( object sender , TextChangedEventArgs e )
        {
            if(IsLoaded == true)            
                IsDirty = true;
        }

        private  void Previousfile_Click ( object sender , RoutedEventArgs e )
        {
            if ( MainWindow . fileslistbox . SelectedIndex > 0 )
            {
                MainWindow . fileslistbox . SelectedIndex -= 1;
                filename = MainWindow . fileslistbox . SelectedItem . ToString ( );
                filename = MainWindow.GetFileforViewer ( filename );
            }
            else 
                return;
            LoadFile (filename );
            if ( haslinenumbers )
                AddLineNumbers ( );
        }

        private void Nextfile_Click ( object sender , RoutedEventArgs e )
        {
            if ( MainWindow . fileslistbox . SelectedIndex < MainWindow . fileslistbox .Items.Count - 1)
            {
                MainWindow . fileslistbox . SelectedIndex += 1;
                filename = MainWindow . fileslistbox . SelectedItem . ToString ( );
                filename = MainWindow . GetFileforViewer ( filename );
            }
            else
                return;
            LoadFile ( filename );
            if ( haslinenumbers )
                AddLineNumbers ( );
        }

        private void Sourcefile_PreviewMouseRightButtonUp ( object sender , MouseButtonEventArgs e )
        {
            SearchWin sw = new (this.Mainwindow,  this);
            sw . Show ( );
            Mainwindow .Searchwin = sw;
        }
        public static void SetupWindowDrag ( Window inst )
        {
            try
            {
                //Handle the button NOT being the left mouse button
                // which will crash the DragMove Fn.....
                MouseButtonState mbs = Mouse . RightButton;
                //Debug. WriteLine ( $"{mbs . ToString ( )}" );
                if ( mbs == MouseButtonState . Pressed )
                    return;
                inst . MouseDown += delegate
                {
                    {
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                        try
                        {
                            inst?.DragMove ( );
                        }
                        catch ( Exception ex )
                        {
                            return;
                        }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    }
                };
            }
            catch ( Exception ex )
            {
                return;
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
        }
    }
}
