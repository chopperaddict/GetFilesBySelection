using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Diagnostics . Eventing . Reader;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Markup;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;

using SortSupportLib;

namespace GetFilesBySelection
{
    /// <summary>
    /// Interaction logic for SearchWin.xaml
    /// </summary>
    public partial class SearchWin : Window
    {
        private FileBrowser Filebrowser { get; set; }
        private MainWindow Mainwindow { get; set; }
        private int CurrentSearchLine { get; set; }
        private int CurrentSearchOffset { get; set; }
        private int Previoussearchoffset { get; set; }
        private int CurrentOffsetCount { get; set; }
        private string searchtext { get; set; } = "";

        public SearchWin ( MainWindow Mainwindow , FileBrowser Parent )
        {
            InitializeComponent ( );
            SetupWindowDrag ( this );
            this . Mainwindow = Mainwindow;
            Filebrowser = Parent;
            searchtxt . Focus ( );
        }

        private void CloseBtn_Click ( object sender , RoutedEventArgs e )
        {
            Filebrowser . Searchwin = null;
            Mainwindow . Searchwin = null;
            this . Close ( );
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

        private void TextBox_TextChanged ( object sender , TextChangedEventArgs e )
        {
            TextBox tb = sender as TextBox;
            if ( tb . Text == "" )
            {
                tb . Foreground = FindResource ( "Black4" ) as SolidColorBrush;
                tb . Text = "Enter search text here ...";
                tb . FontWeight = FontWeight . FromOpenTypeWeight ( 150 );
            }
            else
            {
                tb . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
                tb . FontWeight = FontWeight . FromOpenTypeWeight ( 700 );
            }
        }

        private void TextBox_GotFocus ( object sender , RoutedEventArgs e )
        {
            TextBox tb = sender as TextBox;
            if ( tb . Text == "Enter search text here ..." )
            {
                tb . Text = "";
                tb . UpdateLayout ( );
            }
            tb . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            tb . FontWeight = FontWeight . FromOpenTypeWeight ( 700 );
        }

        private void TextBox_LostFocus ( object sender , RoutedEventArgs e )
        {
            TextBox tb = sender as TextBox;
            if ( tb . Text == "" )
            {
                tb . Text = "Enter search text here ...";
                tb . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
                tb . FontWeight = FontWeight . FromOpenTypeWeight ( 300 );
            }
        }
        private void Search_Click ( object sender , RoutedEventArgs e )
        {
            // WORKING WELL
            CurrentOffsetCount = 0;
            CurrentSearchLine = -1;
            CurrentSearchOffset = 0;
            string txt = searchtxt . Text;
            int offset = 0;
            string outbuffer = Filebrowser . Sourcefile.Text.ToUpper();

            Filebrowser . SourcefileScroller . ScrollToHome ( );
            Filebrowser . SourcefileScroller . UpdateLayout ( );
            Filebrowser . Sourcefile . SelectedText = "";
            Filebrowser . Sourcefile . SelectionLength = 0;
            Filebrowser . Sourcefile . SelectionStart = 0;
            Filebrowser . Sourcefile . UpdateLayout ( );
            searchtext = searchtxt . Text . ToUpper ( );

            offset = outbuffer . IndexOf ( searchtext );
            if ( offset > 0 )
                CurrentSearchOffset = offset;
            else
            {
                MainWindow . PlayErrorBeep ( );
                return;
            }

            if ( offset > -1 )
            {
                Filebrowser . Sourcefile . SelectionStart = offset;
                CurrentSearchOffset = offset;
                Filebrowser . Sourcefile . SelectionLength = searchtext . Length;

                CurrentOffsetCount++;
                // read SourceText in as lines
                string [ ] data = Filebrowser . Sourcefile . Text . ToUpper ( ) . Split ( "\n" );
                // search for 1st match
                int line = -1;

                Filebrowser . SourcefileScroller . LineDown ( );
                Filebrowser . SourcefileScroller . LineDown ( );
                for ( int x = 0 ; x < data . Length ; x++ )
                {
                    outbuffer += data [ x ];
                    Filebrowser . SourcefileScroller . LineDown ( );
                    Filebrowser . Sourcefile . UpdateLayout ( );

                    if ( data [ x ] . ToUpper ( ) . Contains ( searchtext ) == true )
                    {
                        line = x;
                        break;
                    }
                    else
                        line = x;
                }
                Filebrowser . Sourcefile . UpdateLayout ( );

                // Scroll viewer down to show match found line
                if ( line > -1 )
                {
                    // Store currentv line for prev/next operations
                    CurrentSearchLine = line;
                    Filebrowser . Sourcefile . SelectionStart = offset;
                    CurrentSearchOffset = offset;
                    Filebrowser . Sourcefile . SelectionLength = searchtext . Length;
                }
            }
            Filebrowser . Focus ( );
            CurrentOffsetCount = 1;
        }

        private void NextBtn_Click ( object sender , RoutedEventArgs e )
        {
            if ( CurrentOffsetCount < 1 )
                return;
            else
                CurrentSearchOffset += 1;   // move search text pointer fwd 1 character so we do NOT find original item

            int currentmatch = Filebrowser . Sourcefile . SelectionStart;
            if ( currentmatch > 0 )
            {
                CurrentSearchOffset = currentmatch + 1;
                Filebrowser . Sourcefile . SelectionStart = 0;
                Filebrowser . Sourcefile . SelectionLength = 0;
            }
            Filebrowser . SourcefileScroller . ScrollToHome ( );
            Filebrowser . Sourcefile . UpdateLayout ( );

            // re-read data from TextBox control.Text
            string outbuffer = Filebrowser . Sourcefile . Text . ToUpper ( );
            // read Sourcefile in, but as lines
            string [ ] data = Filebrowser . Sourcefile . Text . ToUpper ( ) . Split ( "\n" );

            // Search forward - from start, but for the next match 
            int newoffset = CurrentSearchOffset;
            bool found = false;
            int offset = outbuffer . Substring ( newoffset ) . IndexOf ( searchtext );
            {
                newoffset += offset;
                Filebrowser . Sourcefile . SelectionStart = newoffset;
                Filebrowser . Sourcefile . SelectionLength = searchtext . Length;
                //increment our total searches position
                CurrentOffsetCount++;
                // save latest match starting position 
                CurrentSearchOffset = newoffset;
                found = true;
            }
            if ( found == false )
                return;
            // Now scroll it into view
            outbuffer = "";
            for ( int x = 0 ; x < data . Length ; x++ )
            {
                // Add all data to new buffer
                outbuffer += data [ x ];
                // Have we passed the previous match ??
                if ( outbuffer . Length < CurrentSearchOffset )
                {
                    // scroll down to new matching line - line by line
                    Filebrowser . SourcefileScroller . LineDown ( );
                    Filebrowser . SourcefileScroller . UpdateLayout ( );
                    Filebrowser . Sourcefile . UpdateLayout ( );
                }
            }
            Filebrowser . SourcefileScroller . LineDown ( );
            Filebrowser . SourcefileScroller . UpdateLayout ( );
            Filebrowser . SourcefileScroller . LineDown ( );
            Filebrowser . SourcefileScroller . UpdateLayout ( );
            //Filebrowser . Sourcefile . UpdateLayout ( );
            //Filebrowser . SourcefileScroller . UpdateLayout ( );
            Filebrowser . Focus ( );
        }
        private void PrevBtn_Click ( object sender , RoutedEventArgs e )
        {
            if ( CurrentOffsetCount < 2 )
                return;

            int currentmatch = Filebrowser . Sourcefile . SelectionStart;
            if ( currentmatch > 0 )
            {
                CurrentSearchOffset = currentmatch - 1;
                Filebrowser . Sourcefile . SelectionStart = 0;
                Filebrowser . Sourcefile . SelectionLength = 0;
            }

            Filebrowser . SourcefileScroller . ScrollToHome ( );
            Filebrowser . Sourcefile . UpdateLayout ( );

           //currentmatch = Filebrowser . Sourcefile . SelectionStart;
            //if ( currentmatch > 0 )
            //{
            //    CurrentSearchOffset = currentmatch - 1;
            //}
            Filebrowser . SourcefileScroller . ScrollToHome ( );
            Filebrowser . SourcefileScroller . UpdateLayout ( );
            Filebrowser . Sourcefile . UpdateLayout ( );

            // re-read data from TextBox control.Text
            string outbuffer = Filebrowser . Sourcefile . Text . ToUpper ( ) . Substring ( 0 , CurrentSearchOffset - searchtext . Length );
            int offset = 0;
            bool found = false;
            // Find last previous match
            offset = outbuffer . LastIndexOf ( searchtext );
            if ( offset > 0 )
            {
                Filebrowser . Sourcefile . SelectionStart = offset;
                Filebrowser . Sourcefile . SelectionLength = searchtext . Length;
                found = true;
                CurrentSearchOffset = offset;
                Previoussearchoffset = offset;
                // outbuffer = outbuffer.Substring ( 0, offset- searchtext . Length );
                ScrollLineIntoView ( outbuffer );
                Filebrowser . SourcefileScroller . LineDown ( );
                Filebrowser . SourcefileScroller . UpdateLayout ( );
                Filebrowser . SourcefileScroller . LineDown ( );
                Filebrowser . SourcefileScroller . UpdateLayout ( );
                return;
            }
            else
            {
                Filebrowser . Sourcefile . SelectionStart = Previoussearchoffset;
                Filebrowser . Sourcefile . SelectionLength = searchtext . Length;
                ScrollLineIntoView ( outbuffer );
//                MainWindow . PlayErrorBeep ( );
                return;
            }
            if ( found == true )
            {
                Filebrowser . Sourcefile . SelectionStart = Previoussearchoffset;
                Filebrowser . Sourcefile . SelectionLength = searchtext . Length;
            }
            else
            {
                MainWindow . PlayErrorBeep ( );
                return;
            }
            Filebrowser . SourcefileScroller . LineDown ( );
            Filebrowser . SourcefileScroller . UpdateLayout ( );
            Filebrowser . SourcefileScroller . LineDown ( );
            Filebrowser . SourcefileScroller . UpdateLayout ( );
            Filebrowser . Focus ( );
        }
        private void ScrollLineIntoView ( string buffer )
        {
            Filebrowser . SourcefileScroller . ScrollToHome ( );
            Filebrowser . Sourcefile . UpdateLayout ( );

            string [ ] tmp = buffer . Split ( "\n" );
            for ( int x = 0 ; x < tmp . Length - 1 ; x++ )
            {
                Filebrowser . SourcefileScroller . LineDown ( );
                Filebrowser . SourcefileScroller . UpdateLayout ( );
            }
        }

        private void SearchBtn_KeyDown ( object sender , KeyEventArgs e )
        {
            if ( e . Key == Key . Enter )
                Search_Click ( sender , null );
        }
        private string CreateTempFile ( string filepath )
        {
            if ( filepath . EndsWith ( "\\" ) == false )
                filepath += "\\";
            filepath += "Temp.txt";
            File . WriteAllText ( filepath , Filebrowser . Sourcefile . Text );
            return filepath;
        }
    }
}

