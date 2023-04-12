using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . IO;
using System . Linq;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;
namespace GetFilesBySelection
{
    /// <summary>
    /// Interaction logic for SearchWin.xaml
    /// </summary>
    public partial class SearchWin : Window
    {
        #region  setup variables

        private int totalOffset;
        private FileBrowser Filebrowser { get; set; }
        private MainWindow Mainwindow { get; set; }
        private int CurrentSearchLine { get; set; }
        private int CurrentSearchOffset { get; set; }
        private int Previoussearchoffset { get; set; }
        private int CurrentMatchedCount { get; set; }
        private string searchText { get; set; } = "";
        private string uSearchText { get; set; } = "";

        #endregion setup variables
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

        #region focus handlers
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

        #endregion focus handlers

        #region Search methods
        private void Search_Click ( object sender , RoutedEventArgs e )
        {
            // WORKING WELL
            CurrentMatchedCount = 0;
            CurrentSearchLine = -1;
            CurrentSearchOffset = 0;
            searchText = searchtxt . Text;
            uSearchText = searchtxt . Text . ToUpper ( );
            if ( uSearchText == "Enter search text here ..." || uSearchText == "" )
                return;

            int offSet = 0;
            string outbuffer = Filebrowser . Sourcefile . Text . ToUpper ( );
            string [ ] splt = outbuffer . Split ( "\n" );
            int totalNewLines = splt . Length; // 107 lines for full viewer text ??

            Filebrowser . Sourcefile . IsReadOnly = true;
            Filebrowser . SourcefileScroller . ScrollToHome ( );
            Filebrowser . SourcefileScroller . UpdateLayout ( );
            Filebrowser . Sourcefile . CaretIndex = 0;
            //Filebrowser . Sourcefile . SelectedText = searchtxt.Text;
            Filebrowser . Sourcefile . SelectionStart = 0;
            Filebrowser . Sourcefile . SelectionLength = 0;
            Filebrowser . Sourcefile . UpdateLayout ( );
            offSet = outbuffer . IndexOf ( uSearchText );
            if ( offSet <= 0 )
                return;
            // highlight selected text in main viewer window
            Filebrowser . Sourcefile . SelectionStart = offSet;
            CurrentSearchOffset = offSet;
            Filebrowser . Sourcefile . SelectionLength = searchText . Length;
            // Load data from 0 to x (match start position) with "\n" - NB offset is START of that Line ??
            splt = outbuffer . Substring ( 0 , offSet ) . Split ( "\n" );
            totalNewLines = splt . Length; // 9 for 1st search ??
            if ( offSet > 0 )
                CurrentSearchOffset = offSet;
            else
            {
                MainWindow . PlayErrorBeep ( );
                return;
            }
            // scroll the viewer down to show selection
            for ( int x = 0 ; x <= totalNewLines ; x++ )
            {
                //outbuffer += data [ x ];
                Filebrowser . SourcefileScroller . LineDown ( );
                Filebrowser . Sourcefile . UpdateLayout ( );
            }
            if ( offSet > -1 )
            {
                CurrentMatchedCount++;
            }
            Filebrowser . Focus ( );
            Filebrowser . Sourcefile . IsReadOnly = false;

            CurrentMatchedCount = 1;
        }
        private void NextBtn_Click ( object sender , RoutedEventArgs e )
        {
            // Find NEXT match - if any
            
            // TODO - handle when we hit last match properly
            
            if ( CurrentMatchedCount < 1 )
                return;
            searchText = searchtxt . Text;
            uSearchText = searchtxt . Text . ToUpper ( );
            int currentMatch = Filebrowser . Sourcefile . SelectionStart;
            if ( currentMatch > 0 )
                CurrentSearchOffset = currentMatch;
            Filebrowser . Sourcefile . CaretIndex = 0;

            // scroll to top of scrollviewer again
            Filebrowser . SourcefileScroller . ScrollToHome ( );
            Filebrowser . Sourcefile . UpdateLayout ( );

            // re-read ALL data from TextBox control.Text
            string fullbuff = Filebrowser . Sourcefile . Text;  // std case version 
            string uOutBuffer = Filebrowser . Sourcefile . Text . ToUpper ( );  // uppercase version 
            string tempOutBuffer = "";  //built as we  search, line by line

            // Search forward - from start, but for the next match identified by CurrentMatchedCount.
            int offSet = 0, newOffset = 0;
            CurrentMatchedCount++;
            for ( int i = 0 ; i < CurrentMatchedCount ; i++ )
            {
                offSet = uOutBuffer . Substring ( totalOffset , uOutBuffer . Length - ( totalOffset ) ) . IndexOf ( uSearchText );
                if ( i == 0 )
                    totalOffset += offSet;
                else
                    totalOffset += offSet - 1;
                newOffset = offSet + 1;
                continue;
            }
            totalOffset += 1;

            List<int> cumulativeMatch = new ( );
            string allMatchingData = "";
            int linesCounter = 0, cumulativeLines = 0, matchCounter = 0;

            string [ ] allLines = uOutBuffer . Split ( "\n" );
            for ( int x = 0 ; x < allLines . Length ; x++ )
            {
                if ( allLines [ x ] . Contains ( uSearchText ) == false )
                {
                    // NO match in this line
                    cumulativeLines++;
                    cumulativeMatch . Add ( allLines [ x ] . Length + 1 );
                    tempOutBuffer += allLines [ x ] + "\n";
                    allMatchingData += tempOutBuffer;
                    linesCounter++;
                }
                else
                {
                    // FOUND a match in this line
                    cumulativeLines++;
                    int partMatch = allLines [ x ] . IndexOf ( uSearchText );
                    tempOutBuffer += allLines [ x ] + "\n";
                    allMatchingData += tempOutBuffer;
                    linesCounter++;
                    matchCounter++;
                    if ( matchCounter == CurrentMatchedCount )
                    {
                        // last match, so we have all the data
                        cumulativeMatch . Add ( partMatch );
                        break;
                    }
                    else
                    {
                        cumulativeMatch . Add ( allLines [ x ] . Length + 1 );
                    }
                }
            }
            int fullOffset = 0;
            for ( int i = 0 ; i < cumulativeMatch . Count ; i++ )
            {
                fullOffset += cumulativeMatch [ i ];
            }
            //increment our total searches performed position - looking good.
            string [ ] linestomatch = new string [ 1 ];
            Filebrowser . Sourcefile . SelectionStart = fullOffset;
            Filebrowser . Sourcefile . CaretIndex = fullOffset;
            Filebrowser . Sourcefile . SelectionLength = searchText . Length;
            for ( int x = 0 ; x <= cumulativeLines ; x++ )
            {
                // scroll down to matching line - line by line
                Filebrowser . SourcefileScroller . LineDown ( );
                Filebrowser . SourcefileScroller . UpdateLayout ( );
                Debug . WriteLine ( $"{x}" );
            }

            Filebrowser . Focus ( );
        }
        private void PrevBtn_Click ( object sender , RoutedEventArgs e )
        {
            if ( CurrentMatchedCount < 2 )
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
            Filebrowser . Sourcefile . CaretIndex = 0;
            Filebrowser . SourcefileScroller . ScrollToHome ( );
            Filebrowser . SourcefileScroller . UpdateLayout ( );
            Filebrowser . Sourcefile . UpdateLayout ( );

            // re-read data from TextBox control.Text
            string outbuffer = Filebrowser . Sourcefile . Text . ToUpper ( ) . Substring ( 0 , CurrentSearchOffset - searchText . Length );
            int offset = 0;
            bool found = false;
            // Find last previous match
            offset = outbuffer . LastIndexOf ( searchText );
            if ( offset > 0 )
            {
                Filebrowser . Sourcefile . SelectionStart = offset;
                Filebrowser . Sourcefile . SelectionLength = searchText . Length;
                found = true;
                CurrentSearchOffset = offset;
                Previoussearchoffset = offset;
                ScrollLineIntoView ( outbuffer );
                Filebrowser . SourcefileScroller . LineDown ( );
                Filebrowser . SourcefileScroller . UpdateLayout ( );
                Filebrowser . SourcefileScroller . LineDown ( );
                Filebrowser . SourcefileScroller . UpdateLayout ( );
                //return;
            }
            else
            {
                Filebrowser . Sourcefile . SelectionStart = Previoussearchoffset;
                Filebrowser . Sourcefile . SelectionLength = searchText . Length;
                ScrollLineIntoView ( outbuffer );
                return;
            }
            if ( found == true )
            {
                Filebrowser . Sourcefile . SelectionStart = Previoussearchoffset;
                Filebrowser . Sourcefile . SelectionLength = searchText . Length;
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
        private void SearchBtn_KeyDown ( object sender , KeyEventArgs e )
        {
            if ( e . Key == Key . Enter )
                Search_Click ( sender , null );
        }

        #endregion Search methods

        #region Utility Methods
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
        private string CreateTempFile ( string filepath )
        {
            if ( filepath . EndsWith ( "\\" ) == false )
                filepath += "\\";
            filepath += "Temp.txt";
            File . WriteAllText ( filepath , Filebrowser . Sourcefile . Text );
            return filepath;
        }

        #endregion Utility Methods

    }
}

