using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Diagnostics;
using System . Diagnostics . Eventing . Reader;
using System . IO;
using System . Linq;
using System . Reflection;
using System . Text;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . TextFormatting;

using SortSupportLib;

using static GetFilesBySelection . LoadFilesIteratively;

namespace GetFilesBySelection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool DoSorting = false;
        public static bool Showfull = false;
        public bool ISLOADING = true;
        public bool DOPAUSE { get; set; } = false;
        public static List<string> FulldirectoriesList { get; set; } = new List<string> ( );
        public static ListBox fileslistbox { get; set; }
        public static string Rootpath { get; set; }

        //
        #region PropertyChanged
        //##########################//
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged ( string propertyName )
        {
            try
            {
                if ( this . PropertyChanged != null )
                {
                    var e = new PropertyChangedEventArgs ( propertyName );
                    this . PropertyChanged ( this , e );
                }
            }
            catch ( Exception ex ) { Debug . WriteLine ( $"{ex . Message}" ); }
        }

        //@@@@@@@@@@@@@@@@@@//
        #endregion PropertyChanged
        //@@@@@@@@@@@@@@@@@@//

        public static TextBlock panelinfo { get; set; }
        public static ListBox fileList { get; set; }

        // public static Window window { get; set; }
        public MainWindow ( )
        {
            this . DataContext = this;
            InitializeComponent ( );
            //LoadFilesIteratively . UpdateDirList += new EventHandler<UpdateArgs> ( DoUpdateDir );
        }
        private void scannermain_Loaded ( object sender , RoutedEventArgs e )
        {
            //if ( ISLOADING )
            //    return;
            if ( typespath . Text != "" )
            {
                if ( File . Exists ( typespath . Text ) == false )
                {
                    MessageBox . Show ( $"The file specified does  not exist !" );
                    return;
                }
                filetypeslist . Text = File . ReadAllText ( typespath . Text );
                filetypeslist . Text = filetypeslist . Text . ToUpper ( );
                // remove all trailing \r\n characters
                filetypeslist . Text = StripTrailingLinefeeds ( filetypeslist . Text );
                if ( filetypeslist . Text . Contains ( "\r\n" ) )
                {
                    if ( filetypeslist . Text . Contains ( defsuffix . Text . ToUpper ( ) ) == false )
                    {
                        // reset default suffix if filetypes file has >= 1 entries
                        string [ ] tmp = filetypeslist . Text . Split ( "\r\n" );
                        defsuffix . Text = tmp [ 0 ];
                        defsuffix . UpdateLayout ( );
                    }
                }
                fileslistbox = FilesList;
            }
            else
            {
                filetypeslist . Text = File . ReadAllText ( typespath . Text );
            }

            if ( blockpath . Text != "" )
            {
                if ( File . Exists ( blockpath . Text ) == true )
                {
                    blockedfolderslist . Text = File . ReadAllText ( blockpath . Text );
                    blockedfolderslist . Text = blockedfolderslist . Text . ToUpper ( );
                    // remove all trailing \r\n characters
                    StripTrailingLinefeeds ( blockedfolderslist . Text );
                }
                else
                    MessageBox . Show ( $"The blocked Folders list {blockpath . Text . ToUpper ( )} does not exist ?" );
            }
            panelinfo = InfoPanel;
            fileList = FilesList;
            LoadFilesIteratively . UpdateDirList += new EventHandler<UpdateArgs> ( DoUpdateDir );
            ISLOADING = false;
        }
        private string StripTrailingLinefeeds ( string input )
        {
            string reversed = ReverseString ( input );
            bool proceed = false;
            input = "";
            foreach ( char item in reversed )
            {
                if ( proceed == false && ( item == '\r' || item == '\n' ) )
                    continue;
                else
                {
                    proceed = true;
                    input += item;
                }
            }
            input = ReverseString ( input );
            input += "\r\n";
            return input;
        }
        private void DoUpdateDir ( object sender , UpdateArgs args )
        {
            if ( DOPAUSE == true )
                LoadFilesIteratively . DoQuit ( );
            InfoPanel . Text = args . dirname;
            InfoPanel . UpdateLayout ( );
            //Thread . Sleep ( 20 );
        }

        //public MainWindow ( string [ ] args )
        //{
        //    // NOT CALLED ??
        //    //InitializeComponent ( );
        //    //            LoadFilesIteratively lfi = new ( );
        //    //    LoadFilesIteratively . UpdateDirList += DoUpdateDir;

        //    //lfi . GetSelectedFilesList ( this , InfoPanel ,args );
        //}


        private void rootpath_LostFocus ( object sender , RoutedEventArgs e )
        {
            if ( ISLOADING ) return;
            if ( Directory . Exists ( rootpath . Text ) == false )
                MessageBox . Show ( "The scan path provided does not appear to exist ?.  Please enter a vllaid drive or folder as the root folder for the scan" , "Invalid Scan Path" );
        }
        private void defsuffix_LostFocus ( object sender , RoutedEventArgs e )
        {
            if ( ISLOADING ) return;
            string str = File . ReadAllText ( typespath . Text );
            if ( str . ToUpper ( ) . Contains ( defsuffix . Text . ToUpper ( ) ) == false )
            {
                // Update file types list if new entry not in it
                filetypeslist . Text = StripTrailingLinefeeds ( filetypeslist . Text );
                filetypeslist . Text = defsuffix . Text + "\r\n" + filetypeslist . Text;
                File . WriteAllText ( typespath . Text , filetypeslist . Text );
                filetypeslist . UpdateLayout ( );
            }
        }

        private void outputpath_GotFocus ( object sender , RoutedEventArgs e )
        {
            if ( outputpath . Text != "" )
                outputpath . SelectionLength = outputpath . Text . Length;
        }

        private void typespath_GotFocus ( object sender , RoutedEventArgs e )
        {
            if ( typespath . Text != "" )
                typespath . SelectionLength = typespath . Text . Length;
        }

        private void blockpath_GotFocus ( object sender , RoutedEventArgs e )
        {
            if ( blockpath . Text != "" )
                blockpath . SelectionLength = blockpath . Text . Length;
        }

        private void CloseBtn_Click ( object sender , RoutedEventArgs e )
        {
            LoadFilesIteratively lfi = new ( );
            //            lfi . UpdateDirList -= DoUpdateDirList;
            this . Close ( );
        }

        private void ExecuteBtn_Click ( object sender , RoutedEventArgs e )
        {
            string OutputPath = "";
            FilesList . ItemsSource = null;
            FilesList . Items . Clear ( );
            if ( rootpath . Text == "" )
            {
                ShowErrorMsg ( "You MUST provide a valid Path for the path you want to have scanned for files" , MessageBoxButton . OK );
                return;
            }
            if ( outputpath . Text == "" )
            {
                ShowErrorMsg ( "You MUST provide a valid Path and File name for the destination of the scan results. " ,
                    MessageBoxButton . OK );
                return;
            }
            else
            {
                string path = "";
                string [ ] tmp = outputpath . Text . Split ( $"\\" );
                for ( int x = 0 ; x < tmp . Length - 1 ; x++ )
                {
                    path += $"{tmp [ x ]}\\";
                }
                if ( Directory . Exists ( path ) == false )
                    Directory . CreateDirectory ( path );
                OutputPath = outputpath . Text;
            }
            //if ( typespath . Text == "" )
            //{
            //    MessageBoxResult mbr = MessageBox . Show ( "No path specified for the files types control file (if any) is stored.\n\nThe Scan will return ALL FILES matching THE DEFAULT SUFFIX !!!\n\nAre you quite sure you want to proceeed ?" , "Potentially Long Scan" ,MessageBoxButton . YesNo , MessageBoxImage . Question );
            //        if(mbr == MessageBoxResult.No)
            //        return;
            //    filespec = defsuffix . Text;
            //}
            if ( blockpath . Text == "" )
            {
                if ( ShowErrorMsg ( $"No valid Path for the path to the (Folders to blocked) control file (if any) is stored.\n\n Do you want to continue and have ALL subfolders scanned ?" ,
                    MessageBoxButton . YesNo ) == MessageBoxResult . No )
                    return;

                blockdirs [ 0 ] = "";
            }
            if ( rootpath . Text == outputpath . Text && rootpath . Text == typespath . Text && rootpath . Text == blockpath . Text )
            {
                MessageBoxResult mbr = ShowErrorMsg ( "Are you SURE you want to use the same Path for the input and output paths of the scan  ?" ,
                    MessageBoxButton . YesNo );
                if ( mbr == MessageBoxResult . No )
                    return;
            }
            if ( Directory . Exists ( rootpath . Text ) == false )
            {
                MessageBoxResult mbr = ShowErrorMsg ( $"The Path [{rootpath} does not exist, and cannot therefore be scanned.\n\nPlease select a diffferent path to be scanned" , MessageBoxButton . OK );
                return;
            }
            string [ ] args = new string [ 4 ];
            args [ 0 ] = rootpath . Text;
            string [ ] folders = new string [ 4 ];

            // Create  directories as required

            string [ ] tmp2 = outputpath . Text . Split ( "\\" );
            string outpth = "";
            for ( int x = 0 ; x < tmp2 . Length - 1 ; x++ )
            { outpth += $"{tmp2 [ x ]}\\"; }
            if ( Directory . Exists ( outpth ) == false )
            {
                string [ ] tmp = outputpath . Text . Split ( "\\" );
                for ( int x = 0 ; x < tmp . Length - 1 ; x++ )
                {
                    outpth += $"{tmp [ x ]}\\";
                }
                Directory . CreateDirectory ( outpth );
                outputpath . Text = outpth;
                outputpath . UpdateLayout ( );
                InfoPanel . Text = $"The Path [{outputpath} did  not exist.\n\nHowever, it has now been created for you";
                InfoPanel . Background = FindResource ( "Red5" ) as SolidColorBrush;
                InfoPanel . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            }
            folders [ 0 ] = rootpath . Text . Trim ( ) . ToUpper ( );
            folders [ 1 ] = outputpath . Text . Trim ( ) . ToUpper ( );
            folders [ 2 ] = typespath . Text . Trim ( ) . ToUpper ( );
            folders [ 3 ] = blockpath . Text . Trim ( ) . ToUpper ( );
            args [ 1 ] = folders [ 1 ];
            args [ 2 ] = folders [ 2 ];
            args [ 3 ] = folders [ 3 ];

            CancelBtn . IsEnabled = true;
            ExecuteBtn . IsEnabled = false;
            CloseBtn . IsEnabled = false;
            FulldirectoriesList . Clear ( );
            ExecuteBtn . UpdateLayout ( );
            CancelBtn . UpdateLayout ( );
            CloseBtn . UpdateLayout ( );
            LoadFilesIteratively lfi = new ( );
            string [ ] allresults = null;
            InfoPanel . Text = $"Please wait, Scanning files system from root folder {RootPath} (through all folders specified...";
            InfoPanel . Background = FindResource ( "Orange4" ) as SolidColorBrush;
            InfoPanel . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            InfoPanel . UpdateLayout ( );
            Filecount . Text = $"Scan in progress ....";
            Filecount . UpdateLayout ( );
            Mouse . OverrideCursor = Cursors . Wait;
            if ( typespath . Text . Trim ( ) == "" )
            {
                //********************************************************************************//
                string [ ] validfile = new string [ 1 ];
                validfile [ 0 ] = defsuffix . Text;
                allresults = lfi . GetSelectedFilesList ( validfile , InfoPanel , args );
                //********************************************************************************//
            }
            else
            {
                //********************************************************************************//
                string [ ] validfile = File . ReadAllLines ( typespath . Text );
                validfile [ 0 ] = defsuffix . Text;
                allresults = lfi . GetSelectedFilesList ( validfile , InfoPanel , args );
                //********************************************************************************//
            }

            List<string> alldirs = FulldirectoriesList;

            Debug . WriteLine ( $"All done {allresults . Length} files identified..." );
            FilesList . ItemsSource = null;
            FilesList . Items . Clear ( );
            FilesList . ItemsSource = allresults;
            Filecount . Text = $"{FilesList . Items . Count} files from {FulldirectoriesList . Count} folders identified matching your specifications...";
            InfoPanel . Text = $"{FilesList . Items . Count} files (sorted alphabetically) from {FulldirectoriesList . Count} folders identified matching your specifications...";
            InfoPanel . Background = FindResource ( "Green5" ) as SolidColorBrush;
            InfoPanel . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            int dircount = FulldirectoriesList . Count;
            CancelBtn . IsEnabled = false;
            ExecuteBtn . IsEnabled = true;
            CloseBtn . IsEnabled = true;
            Mouse . OverrideCursor = Cursors . Arrow;
            Debug . WriteLine ( $"Duplicates found = {LoadFilesIteratively . duplicates . Count}" );
        }
        private static MessageBoxResult ShowErrorMsg ( string msg , MessageBoxButton mbtn )
        {
            MessageBoxResult mbr = MessageBox . Show ( msg , "Invalld input" , mbtn );
            if ( mbr == MessageBoxResult . No )
                return MessageBoxResult . No;
            else
                return MessageBoxResult . Yes;
        }

        private void DoSort_Click ( object sender , RoutedEventArgs e )
        {
            if ( DoSort . IsChecked == true )
            {
                if ( FilesList . Items . Count > 0 )
                {
                    Mouse . OverrideCursor = Cursors . Wait;
                    string [ ] sorting = new string [ FilesList . Items . Count ];
                    for ( int x = 0 ; x < FilesList . Items . Count ; x++ )
                    { sorting [ x ] = FilesList . Items [ x ] . ToString ( ); }
                    Array . Sort ( sorting , new SortString ( ) );
                    FilesList . ItemsSource = sorting;
                    FilesList . UpdateLayout ( );
                    InfoPanel . Text = "Results have been sorted into Alpabetical order for you..";
                    InfoPanel . Background = FindResource ( "Red3" ) as SolidColorBrush;
                    InfoPanel . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    Mouse . OverrideCursor = Cursors . Arrow;
                }
                DoSorting = true;
            }
            else
            {
                if ( FilesList . Items . Count > 0 )
                {
                    InfoPanel . Text = "Sorry, CANNOT UNSORT from Alpabetical order. Run Scan again with this option unchecked ..";
                    InfoPanel . Background = FindResource ( "Red3" ) as SolidColorBrush;
                    InfoPanel . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    PlayErrorBeep ( );
                }
                DoSorting = false;
            }
        }

        private void CancelBtn_Click ( object sender , RoutedEventArgs e )
        {
            LoadFilesIteratively . DoQuit ( );
            Mouse . OverrideCursor = Cursors . Arrow;
        }

        #region validation
        private void typespath_Lostfocus ( object sender , RoutedEventArgs e )
        {
            if ( typespath . Text != "" )
            {
                if ( File . Exists ( typespath . Text ) )
                    filetypeslist . Text = File . ReadAllText ( typespath . Text );
                else
                {
                    MessageBox . Show ( $"The specified file types list {typespath . Text . ToUpper ( )} does not exist ?\n\nPlease provide a valid file/Path or remove the entry alltogether, and try again, " , "Invalid File Specifed" , MessageBoxButton . OK , MessageBoxImage . Stop );
                    filetypeslist . Text = "";
                    filetypeslist . UpdateLayout ( );
                    e . Handled = true;
                    //typespath . Focus ( );
                }
            }
        }

        private void blockpath_Lostfocus ( object sender , RoutedEventArgs e )
        {
            if ( blockpath . Text != "" )
            {
                if ( File . Exists ( blockpath . Text ) )
                    blockedfolderslist . Text = File . ReadAllText ( blockpath . Text );
                else
                {
                    MessageBoxResult mbr = MessageBox . Show ( $"The specified \"Blocked folders\"list {blockpath . Text . ToUpper ( )} does not exist ?\n\nThis will mean ALL folders below the root folder will be scanned.\n\nAre you quite sure that is what you want ?" , "Invalid File Path/Name identified" , MessageBoxButton . YesNo , MessageBoxImage . Question );
                    if ( mbr == MessageBoxResult . No )
                        return;
                    else
                    {
                        blockedfolderslist . Text = "";
                        blockedfolderslist . UpdateLayout ( );
                    }
                }
            }
        }

        private void outputpath_LostFocus ( object sender , RoutedEventArgs e )
        {
            if ( outputpath . Text != "" )
            {
                string [ ] tmp = outputpath . Text . Split ( "\\" );
                string path = "";
                for ( int x = 0 ; x < tmp . Length - 1 ; x++ )
                {
                    path += tmp [ x ] + "\\";
                }
                if ( Directory . Exists ( path ) == false )
                {
                    MessageBoxResult mbr = MessageBox . Show ( $"The specified output folder {outputpath . Text . ToUpper ( )} does not exist ?.\n\nDo you  want it created now ?" , "File Path Error" , MessageBoxButton . YesNo );
                    if ( mbr == MessageBoxResult . Yes )
                    {
                        var result = Directory . CreateDirectory ( outputpath . Text );
                        if ( result == null )
                            MessageBox . Show ( $"Output folder {outputpath . Text . ToUpper ( )} cannot be created ?.\n\nPlease check it is a valid  path and try again?" );
                    }
                }
            }
        }

        #endregion validation

        private void DataChanged_click ( object sender , RoutedEventArgs e )
        {
            if ( ISLOADING ) return;
            string str = "";
            bool updated = false;
            TextBox tb = sender as TextBox;
            if ( tb != null )
            {
                // the top field has been changed update listbox
                updated = false;
                if ( tb . Name . ToLower ( ) == "defsuffix" )
                {
                    if ( defsuffix . Text == "" )
                    {
                        MessageBox . Show ( "There MUST be at least one file type suffix specified for the scan system to be able to work!\n\nPlease provide a valid entry for this (eg: \".TXT\"" , "Invalid File specification" , MessageBoxButton . OK , MessageBoxImage . Stop );
                        return;
                    }
                    str = filetypeslist . Text;
                    string [ ] tmp = str . Split ( "\r\n" );
                    if ( tmp [ 0 ] != defsuffix . Text )
                    {
                        bool found = false;
                        for ( int x = 0 ; x < tmp . Length ; x++ )
                        {
                            if ( tmp [ x ] == defsuffix . Text )
                            {
                                found = true;
                                break;
                            }
                        }
                        // add field entry to top of file types list
                        if ( found == true )
                            return;
                        filetypeslist . Text = defsuffix . Text + "\r\n" + filetypeslist . Text;
                        filetypeslist . UpdateLayout ( );
                        MessageBox . Show ( $"The default file type suffix specified for the scan system has been added to the File Types reference file {filetypeslist . Text}" , "File Specification Changed" , MessageBoxButton . OK , MessageBoxImage . Stop );
                        return;
                    }
                }
                else if ( tb . Name . ToLower ( ) == "rootpath" )
                {
                    if ( Directory . Exists ( rootpath . Text ) == false )
                    {
                        MessageBox . Show ( "The root path for the next scan does not exist !\n\nPlease enter a valid path" , "Invalid Path" , MessageBoxButton . OK , MessageBoxImage . Stop );
                        return;
                    }
                    Rootpath = rootpath.Text;
                }
                else if ( tb . Name . ToLower ( ) == "outputpath" )
                {
                    if ( File . Exists ( outputpath . Text ) == false )
                    {
                        MessageBox . Show ( "The path for the scan does results does not exist !\n\nPlease enter a valid path for them" , "Invalid Path" , MessageBoxButton . OK , MessageBoxImage . Stop );
                        return;
                    }
                }
                else if ( tb . Name . ToLower ( ) == "typespath" )
                {
                    ////// file types FIELD changed - update list
                    if ( typespath . Text == "" )
                    {
                        filetypeslist . Text = "";
                        filetypeslist . UpdateLayout ( );
                        defsuffix . Text = "*.*";
                        defsuffix . UpdateLayout ( );
                        return;
                    }
                    if ( File . Exists ( typespath . Text ) == false )
                    {
                        if ( typespath . Text != "" )
                        {
                            MessageBox . Show ( "The path for the scan file types does NOT exist !\n\nPlease enter a valid path for this file or remove the entry altogether" , "Invalid Path" , MessageBoxButton . OK , MessageBoxImage . Stop );
                            filetypeslist . Text = "";
                            filetypeslist . UpdateLayout ( );
                            return;
                        }
                    }
                    else
                    {
                        filetypeslist . Text = File . ReadAllText ( typespath . Text );
                        // add default suffix if not in list already
                        if ( filetypeslist . Text . Contains ( defsuffix . Text ) == false )
                            filetypeslist . Text = defsuffix . Text + "\r\n" + filetypeslist . Text;
                        filetypeslist . UpdateLayout ( );
                    }
                }
                else if ( tb . Name . ToLower ( ) == "blockpath" )
                {
                    if ( blockpath . Text != "" )
                    {
                        if ( File . Exists ( blockpath . Text ) )
                            blockedfolderslist . Text = File . ReadAllText ( blockpath . Text );
                        else
                        {
                            MessageBoxResult mbr = MessageBox . Show ( $"The specified \"Blocked folders\"list {blockpath . Text . ToUpper ( )} does not exist ?\n\nThis will mean ALL folders below the root folder will be scanned.\n\nAre you quite sure that is what you want ?" , "Invalid File Path/Name identified" , MessageBoxButton . YesNo , MessageBoxImage . Question );
                            if ( mbr == MessageBoxResult . No )
                                return;
                            else
                            {
                                blockedfolderslist . Text = "";
                                blockedfolderslist . UpdateLayout ( );
                            }
                        }
                    }
                    else
                        blockedfolderslist . Text = "";
                }
                else if ( tb . Name . ToLower ( ) == "filetypeslist" )
                {
                    if ( tb . IsFocused == false )
                    {
                        // loosing focus , check defaullt suffix
                        bool found = false;
                        string [ ] tmp3 = filetypeslist . Text . Split ( "\r\n" );
                        foreach ( var item in tmp3 )
                        {
                            if ( defsuffix . Text == item )
                            {
                                found = true;
                                break;
                            }
                        }
                        if ( found == false )
                        {
                            defsuffix . Text = tmp3 [ 0 ];
                            MessageBox . Show ( $"The default file suffix has been updated for you from the file types list." , "File Specification Changed" , MessageBoxButton . OK , MessageBoxImage . Information );
                            return;
                        }
                        // update files list file- JIC
                        File . WriteAllText ( typespath . Text , filetypeslist . Text );
                    }
                    // the file types list may have been changed - update top default suffix field as top of the list entry- WORKING
                    string [ ] tmp = filetypeslist . Text . Split ( "\r\n" );
                    defsuffix . Text = tmp [ 0 ] . Substring ( 0 , tmp [ 0 ] . Length );
                    defsuffix . UpdateLayout ( );
                }
                else if ( tb . Name . ToLower ( ) == "blockedfolderslist" )
                {
                    // the file types list has been changed - update top field - WORKING
                    if ( blockpath . Text == "" )
                    {
                        blockedfolderslist . Text = "";
                        return;
                    }
                    str = File . ReadAllText ( blockpath . Text );
                    if ( str != blockedfolderslist . Text )
                    {
                        File . WriteAllText ( blockpath . Text , blockedfolderslist . Text );
                        MessageBox . Show ( $"The Blocked folders file has been updated for you." , "File Specification Changed" , MessageBoxButton . OK , MessageBoxImage . Information );
                        return;
                    }
                }
            }
        }
        public static string ReverseString ( string input )
        {
            string output = "";
            for ( int x = input . Length - 1 ; x >= 0 ; x-- )
                output += input [ x ];
            return output;
        }

        private void Showfull_Click ( object sender , RoutedEventArgs e )
        {
            if ( Showfullpath . IsChecked == true )
            {
                Mouse . OverrideCursor = Cursors . Wait;
                string [ ] sorting = new string [ FilesList . Items . Count ];
                for ( int x = 0 ; x < FilesList . Items . Count ; x++ )
                {
                    if ( FilesList . Items [ x ] . ToString ( ) . Contains ( rootpath . Text ) == true )
                        break;
                    if ( rootpath . Text . EndsWith ( "\\" ) )
                        sorting [ x ] = rootpath . Text + FilesList . Items [ x ] . ToString ( ) . Substring ( 3 );
                    else
                        sorting [ x ] = rootpath . Text + "\\" + FilesList . Items [ x ] . ToString ( ) . Substring ( 4 );
                }
                FilesList . ItemsSource = sorting;
                FilesList . UpdateLayout ( );
                Mouse . OverrideCursor = Cursors . Arrow;
                Rootpath = rootpath.Text;

                Showfull = true;
            }
            else
            {
                Mouse . OverrideCursor = Cursors . Wait;
                string [ ] sorting = new string [ FilesList . Items . Count ];
                for ( int x = 0 ; x < FilesList . Items . Count ; x++ )
                {
                    if ( FilesList . Items [ x ] . ToString ( ) . Contains ( rootpath . Text ) == false )
                        break;
                    sorting [ x ] = "..." + FilesList . Items [ x ] . ToString ( ) . Substring ( rootpath . Text . Length , FilesList . Items [ x ] . ToString ( ) . Length - rootpath . Text . Length );
                }
                FilesList . ItemsSource = sorting;
                FilesList . UpdateLayout ( );
                Showfull = false;
                Rootpath = rootpath . Text;
                Mouse . OverrideCursor = Cursors . Arrow;
            }
        }
        public static void PlayErrorBeep ( int freq = 280 , int count = 100 , int repeat = 1 )
        {
            //List<Task<bool>> list = new List<Task<bool>> ( );
            Dobeep ( 320 , 300 );
            Dobeep ( 260 , 800 );
            return;
        }
        public static void Dobeep ( int freq , int duration )
        {
            Console . Beep ( freq , duration );
            //return true;
        }
        public string GetFullFileName ( int index )
        {
            string file = "";
            FilesList . SelectedIndex += index;
            file = FilesList . SelectedItem . ToString ( );
            if ( file . Contains ( "..." ) && RootPath . Contains ( "\\" ) == true )
                file = RootPath + file;
            else if ( file . Contains ( "..." ) )
                file = RootPath + "\\" + file;
            return file;
        }
        private void FilesList_MouseDoubleClick ( object sender , MouseButtonEventArgs e )
        {
            string file = fileslistbox . SelectedItem . ToString ( );
            if ( Rootpath == null || Rootpath == "" )
            {
                Rootpath = rootpath . Text;
            }
            file = GetFileforViewer ( file );
            FileBrowser sfb = new FileBrowser ( file );
            sfb . Show ( );
        }
        public static string GetFileforViewer (string file )
        {
            if ( file . Contains ( "..." ) == true )
            {
                if ( Rootpath . EndsWith ( "\\" ) )
                    file = Rootpath + file . Substring ( 5 );
                else
                    file = Rootpath + "\\" + file . Substring ( 5 );
            }
            return file;
        }
    }
}