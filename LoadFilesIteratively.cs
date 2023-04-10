using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Diagnostics;
using System . IO;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Threading;

using SortSupportLib;

namespace GetFilesBySelection
{
    public class LoadFilesIteratively
    {
        public static bool DOSTOP = false;
        public const bool DOFILEPROCESSING = false;

        // define the delegate handler signature and the event that will be raised
        // to send the change of directory being searched so mainwindow can update panel
        public delegate void UpdateDirlistingHandler ( object sender , UpdateArgs args );

        public static event EventHandler<UpdateArgs>? UpdateDirList;

        public static List<string> duplicates = new ( );
        /// public event UpdateDirlistingHandler UpdateDirList;
        //public event EventHandler? UpdateDirList;
        protected virtual void OnUpdateDirList ( object sender , UpdateArgs args )
        {
            if ( UpdateDirList != null )
                UpdateDirList ( sender , args );
        }

        #region setup
#pragma warning disable CA2211
        public static Window? win { get; set; }
        // The code that's violating the rule is on this line.
        public static string underline = "==============================================================================================";
        public static int Inbuff;
        public static int TotalFiles;
        public static int indexincrement;
        public static int TotalLines;
        public static int TotalMethods;
        public static int TotalExErrors;
        public static int match;
        public static int linescount;
        public static bool found;
        public static bool isfullcomment;
        public static string path = "";
        public static string? searchpath;
        public static string FullOutputLine = "";
        public static int AllProcsIindex;
        public static int WalkIterationcount = 0;
        public static List<string> UniquePathsOnly { get; set; } = new ( );
        private static bool CreateIterativeReport = false;
        public static string? filespec { get; set; }
        public static Dictionary<string , string> AllMethods = new ( );
        public static List<Tuple<int , string? , string?>> DebugErrors = new ( );
        public static List<Tuple<int , string , string>> ErrorPaths = new ( );
        public static List<Tuple<int , string , string>> AllValidEntries = new ( );

        // declare tuple array (dummy size = 5000) !!
        public static Tuple<int , string , string , string [ ]> [ ] Alltuples = new Tuple<int , string , string , string [ ]> [ 5000 ];
        public static List<Tuple<int , string , string , string [ ]>> AllTupesList = new ( );
        public static int DirectoryCount { get; set; }
        public string [ ] validtypes { get; set; } = new string [ 1 ];
        public string [ ] blocktypes { get; set; } = new string [ 1 ];
        public static int tbindex { get; set; }
        public static int errorindex { get; set; }
        public static StringBuilder outputpublic { get; set; } = new ( );
        public static StringBuilder outputinternal { get; set; } = new ( );
        public static StringBuilder outputprivate { get; set; } = new ( );
        public static string RootPath { get; set; } = "";
        public static List<string> FullPathFileNameList { get; set; } = new ( );
        public static string [ ] AllFileNames = new string [ 1000 ];
        public static List<string> FullfilesList { get; set; } = new List<string> ( );
//        public static List<string> FulldirectoriesList { get; set; } = new List<string> ( );
        public static string? buffer { get; set; }

        public static string defaultsuffix = "*.cs";
        //        public static string [ ] blocktypes { get; set; } = new string [ 1 ];
        public static string [ ] blockdirs { get; set; } = new string [ 1 ];
        public static string filepath = "";
        public static string searchfile = "";
        public static string fname = "";
        public static string [ ] DictResults = new string [ 2 ];
        public static string? capsbuffer { get; set; }
        public static string? CurrentFile { get; set; }
        public static string [ ]? lines { get; set; }
        public static string [ ]? upperlines { get; set; }
        public static string pathfilename { get; set; } = "";
        public string DEFAULTPATH = "";
        public static string errmsg = "";
        public Tuple<int , string , string>? tuple;
        // List<Tuple<int , string , string>> DebugErrors { get; set; }  = new ( );

#pragma warning restore CA2211
        #region PropertyChanged
        //##########################//
        public event PropertyChangedEventHandler? PropertyChanged;
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
        #endregion PropertyChanged

        #endregion  setup

        private void DoUpdateDir ( UpdateArgs args )
        {
            throw new NotImplementedException ( );
        }

        public static void DoQuit ( )
        {
            DOSTOP = true;
        }
        public string [ ] GetSelectedFilesList ( string [ ] srchtext , TextBlock infopanel , string [ ] args )
        {
            string [ ] validtypes = new string [ srchtext . Length ];
            string [ ] blocktypes = new string [ 1 ];
            string [ ] blockdirs = new string [ 1 ];
            string outpath = "", FullOutputPath = "";
            int cnt = 0;
            string argstring = "";
            string [ ] sorting = new string [ 1 ];

            //win = Win;
            //errmsg = "";
            // tuple = Tuple . Create ( -1 , pathfilename , errmsg );
            if ( args . Length == 0 )
            {
                Console . Write ( "Enter filename : " );
                argstring = Console . ReadLine ( );
                if ( argstring == "" )
                    argstring = @"C:\";
                validtypes [ 0 ] = "*.*";
            }
            else
                validtypes = srchtext;

            if ( args . Length >= 1 )
            {
                argstring = args [ 0 ];
            }
            if ( args . Length >= 2 )
            {
                // handle output path
                if ( File . Exists ( args [ 1 ] ) == false )
                {
                    string path = "";
                    string [ ] tmp = args [ 1 ] . Split ( $"\\" );
                    if ( tmp . Length < 3 )
                    {
                        MessageBoxResult mbr = MessageBox . Show ( $"The output Path/File [{args [ 1 ]} specified does not appear to be a valid Path and filename for the output of the Scan.\n\nPlease enter a fully qualified path and filename for the scanned output..." , "" , MessageBoxButton . OK );
                        return null;

                    }
                    for ( int x = 0 ; x < tmp . Length - 1 ; x++ )
                    {
                        if ( tmp [ x ] . Contains ( "." ) == false )
                            path += $"{tmp [ x ]}\\";
                    }
                    if ( Directory . Exists ( path ) == false )
                        Directory . CreateDirectory ( path );
                    else
                    {

                        path += $"{tmp [ tmp . Length - 1 ]}";
                    }
                    outpath = path;
                    FullOutputPath = args [ 1 ];
                }
                else
                {
                    outpath = args [ 1 ];
                    FullOutputPath = args [ 1 ];
                }

                if ( args [ 2 ] != "" && File . Exists ( args [ 2 ] ) == false )
                {
                    string path = "";
                    string [ ] tmp = args [ 2 ] . Split ( $"\\" );
                    if ( tmp . Length < 3 )
                    {
                        MessageBoxResult mbr = MessageBox . Show ( $"The required File Types Path/File [{args [ 1 ]} specified does not appear to exist.\n\nPlease enter a fully qualified path and filename for the scanned output..." , "" , MessageBoxButton . OK );
                        return null;
                    }
                    path = "";
                    for ( int x = 0 ; x < tmp . Length - 1 ; x++ )
                    {
                        if ( tmp [ x ] . Contains ( "." ) == false )
                            path += $"{tmp [ x ]}\\";
                    }
                    if ( Directory . Exists ( path ) == false )
                    {
                        Directory . CreateDirectory ( path );
                        infopanel . Text = $"New Directory [ {path . ToUpper ( )} ] created for you ....";
                    }
                    path += $"{tmp [ tmp . Length - 1 ]}";
                    if ( File . Exists ( path ) == false )
                    {
                        File . WriteAllText ( path , "" );
                        infopanel . Text = $"New File Types file [ {path . ToUpper ( )} ] created for you ....";
                    }
                    validtypes = File . ReadAllLines ( path );
                }
                //else if ( args [ 2 ] != "" )
                //    validtypes = File . ReadAllLines ( args [ 2 ] );
                else
                {
                    // validtypes = new string [ 1 ];
                    validtypes = srchtext;
                }
                //if ( srchtext == "*.*" )
                //{
                //    validtypes = new string [ 1 ];
                //    validtypes [ 0 ] = srchtext;
                //}
                if ( args [ 3 ] != "" && File . Exists ( args [ 3 ] ) == false )
                {
                    string path = "";
                    string [ ] tmp = args [ 3 ] . Split ( $"\\" );
                    if ( tmp . Length < 3 )
                    {
                        MessageBoxResult mbr = MessageBox . Show ( $"The required Block Types Path/File [{args [ 1 ]} specified does not appear to exist.\n\nPlease enter a fully qualified path and filename for this element..." , "" , MessageBoxButton . OK );
                        return null;
                    }
                    path = "";
                    for ( int x = 0 ; x < tmp . Length - 1 ; x++ )
                    {
                        if ( tmp [ x ] . Contains ( "." ) == false )
                            path += $"{tmp [ x ]}\\";
                    }
                    if ( Directory . Exists ( path ) == false )
                    {
                        Directory . CreateDirectory ( path );
                        infopanel . Text = $"New Directory [ {path . ToUpper ( )} ] created for you ....";
                    }
                    path += $"{tmp [ tmp . Length - 1 ]}";
                    if ( File . Exists ( path ) == false )
                    {
                        File . WriteAllText ( path , "" );
                        infopanel . Text = $"New Block Folders file [ {path . ToUpper ( )} ] created for you ....";
                    }
                    blockdirs = File . ReadAllLines ( path );
                }
                else if ( args [ 3 ] != "" )
                    blockdirs = File . ReadAllLines ( args [ 3 ] );
                else
                    blockdirs [ 0 ] = "";
            }

            //LoadFilesIteratively lf = new ( );
            FullPathFileNameList . Clear ( );
            MainWindow.FulldirectoriesList . Clear ( );
            Mouse . OverrideCursor = Cursors . Wait;

            //Task task = Task . Run ( ( ) =>
            //{
            //    Application.Current. Dispatcher . Invoke ( async ( ) =>
            //    {
            FullPathFileNameList = LoadAllFiles ( outpath , argstring , validtypes , blockdirs );
            //    } );
            //} );
            //*************************************************************************************************//
            //            FullPathFileNameList = LoadAllFiles ( outpath , argstring , validtypes , blockdirs );
            //*************************************************************************************************//

            string resultsfile = @$"{FullOutputPath}";
            File . WriteAllText ( resultsfile , "Files Scanner Results :-\n" );
            File . AppendAllText ( resultsfile , $"Scan of root folder {RootPath} identified {FullPathFileNameList . Count . ToString ( )} matching (unblocked) files ....\n" );
            File . AppendAllText ( resultsfile , $"Excluded folders were :\n" );
            foreach ( var item in blockdirs )
            {
                File . AppendAllText ( resultsfile , $"{item}\n" );
            }
            File . AppendAllText ( resultsfile , $"\nFile Types required were :\n" );
            foreach ( var item in validtypes )
            {
                File . AppendAllText ( resultsfile , $"{item}\n" );
            }
            sorting = new string [ FullPathFileNameList . Count ];
            File . AppendAllText ( resultsfile , $"\nFiles identified were :\n\n" );
            cnt = 0;
            foreach ( var item in FullPathFileNameList )
            {
                sorting [ cnt++ ] = $"{item}";
            }

            if ( MainWindow . DoSorting == true )
            {
                // return files list in alpha order
                Array . Sort ( sorting , new SortString ( ) );
            }
            foreach ( var item in sorting )
            {
                File . AppendAllText ( resultsfile , $"{item}\n" );
            }

            File . AppendAllText ( resultsfile , $"\n     E.O.F....\n" );
            Mouse . OverrideCursor = Cursors . Arrow;
            return sorting;
        }

        public List<string> LoadAllFiles ( string outpath , string rootpath , string [ ] validtypes , string [ ] blockdirs )
        {
            int TotalFiles = 0, linescounter = 0;
            if ( validtypes == null || validtypes . Length == 0 )
            {
                validtypes = new string [ 1 ];
                validtypes [ 0 ] = "";
            }
            if ( validtypes [ 0 ] == null )
                validtypes [ 0 ] = "";
            if ( validtypes [ 0 ] == "" )
                validtypes [ 0 ] = "*.*";

            if ( blockdirs == null || blockdirs . Length == 0 )
            {
                blockdirs = new string [ 1 ];
                blockdirs [ 0 ] = "";
            }
            if ( blockdirs [ 0 ] == null )
                blockdirs [ 0 ] = "";
            if ( blockdirs [ 0 ] == "" )
                blockdirs [ 0 ] = ".GIT";

            string [ ] arguments;
            string upperline = "";
            bool IsDupe = false;
            filespec = validtypes [ 0 ];
            if ( filespec == null )
                filespec = "*.CS";
            RootPath = rootpath;
            if ( outpath == "" )
                path = $@"{RootPath}\output\AllScanFiles.txt";
            else
                path = outpath;

            #region setup2
            //***************************//
            // Get user entry path details
            //***************************//
            if ( rootpath . Length == 0 )
            {
                return FullPathFileNameList;
                //    FullPathFileNameList;
            }
            else
            {
                // off we go to find all matches
                DEFAULTPATH = @"C:\wpfmain\";
                RootPath = rootpath . Trim ( );
                // no digits or commans identified, must be  a file name  ?
                if ( rootpath . ToUpper ( ) . Contains ( ".CS" ) )
                {
                    // Its a source file !!
                    searchfile = GetFilenameFromPath ( RootPath . Trim ( ) , out filepath );
                    AllFileNames [ 0 ] = RootPath;
                    FullPathFileNameList . Add ( RootPath );
                    //Debug . WriteLine ( $"" );
                    // PATH+NAME of only file in array
                    pathfilename = RootPath;
                    TotalFiles = 1;
                    return FullPathFileNameList;
                }
                else
                {
                    // its a path
                    // PATH+NAME of 1st file in array
                    // Get iterative list of dirs and files ??
                    //TotalFiles = DoSetup ( argstring );
                    int counter = 0;
                    WalkIterationcount = 0;
                    foreach ( var item in validtypes )
                    {
                        // pass root directory, a file type from [], blockfolders []

                        //******************************************************************//
                        counter = DoLoadAllFiles ( RootPath , item , blockdirs );
                        //******************************************************************//
                        //Debug . WriteLine ( $"En Route with {item} : total files = {FullPathFileNameList . Count}" );

                        TotalFiles += counter;
                        WalkIterationcount++;
                        //Debug . WriteLine ( $"counter = {counter},  after {item} = {TotalFiles} / {FullPathFileNameList . Count}, iterations = {WalkIterationcount}" );
                    }
                }
                Console . WriteLine ( $"Returning a  total of {FullPathFileNameList . Count} valid matching files" );
                // FINAL RESULT - RETURN TO CALLER AS LIST<STRING>
                //foreach ( var item in FullPathFileNameList )
                //{
                //Debug . WriteLine ( $"Final : total files = {FullPathFileNameList . Count}" );
                //}
                return FullPathFileNameList;

            }
            #endregion setup2
        }

        public int WalkDirectoryTree ( DirectoryInfo root , string filetype , ref int totalfiles , string [ ] blockdirs )
        {
            // root will be changed when it is called iteratively
            int filesindex = 0;
            string filespec = "";
            FileInfo [ ] files = new FileInfo [ 1 ];
            DirectoryInfo [ ] subDirs = new DirectoryInfo [ 1 ];
            filespec = filetype;
            if ( filespec == "" )
                return 0;
            // First, process all the files directly under this folder
            // or the  current folder when iterating
            try
            {
                bool isok = true;
                int counter = 0;
                files = root . GetFiles ( filespec );

                Debug . WriteLine ( $"Parsing {root} for {filespec}..." );
     //           Debug . Assert ( root . FullName.Contains ( "Batchfiles" ) == false);
                foreach ( FileInfo item in files )
                {
                    isok = true;
                    if ( DOSTOP == true ) break;
                    if ( blockdirs != null )
                    {
                        for ( int y = 0 ; y < blockdirs . Length ; y++ )
                        {
                            // check block file 1st
                            if ( item . Name . ToUpper ( ) . Contains ( blockdirs [ y ] ) == false )
                            {
                                // Check for duplicates
                                //string balname = "..." + item . FullName . Substring ( RootPath . Length + 3 );
                                counter = AddFilesToList ( item . FullName , ref filesindex );
                                MainWindow . fileList . Items . Add ( item . FullName );
                                MainWindow . fileList . SelectedIndex = MainWindow . fileList . Items . Count - 1;
                                MainWindow . fileList . UpdateLayout ( );
                                totalfiles += counter;
                            }
                            else
                                Debug . WriteLine ( $"Duplicate found : {item . FullName}" );
                        }
                    }
                    else
                    {
                        //string balname = "..." + item . FullName . Substring ( RootPath . Length + 3 );
                        counter = AddFilesToList ( item . FullName , ref filesindex );
                        totalfiles += counter;
                    }
                }
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch ( UnauthorizedAccessException e )
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                Debug . WriteLine ( $"\nERROR  426 : {e . Message}" );
            }

            catch ( DirectoryNotFoundException e )
            {
                Debug . WriteLine ( "\nERROR 431 : {e . Message}" );
            }

            //if ( files != null && files . Length > 0 )
            //{
            //    try
            //    {
            //        int counter = AddFilesToList ( files , ref filesindex );

            //        Debug . WriteLine ( $"{files . Length} files added to lists" );
            //        // we have 86 files  for wpfmain
            //        if ( DOSTOP == false )
            //        {
            //            if ( CreateIterativeReport )
            //            {
            //                foreach ( FileInfo fi in files )
            //                {

            //                    // If we want to open, delete or modify the file, then
            //                    // thea try-catch block is required here to handle the case
            //                    // where the file has been deleted since the call to TraverseTree().

            //                    string Files = fi . FullName . ToUpper ( );
            //                    if ( Files . Contains ( ".G.CS" ) || Files . Contains ( "ASSEMBLY" ) )
            //                        continue;
            //                    else
            //                    {
            //                        FullfilesList . Add ( fi . FullName );
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch ( Exception ex )
            //    {
            //        //Add entry to Error Tuple collection 
            //        //Tuple<int , string , string> t = Tuple . Create ( -1 , pathfilename , $"\n{ex . Message}" );
            //        //Program . DebugErrors . Add ( t );
            //        Debug . WriteLine ( $"\nERROR Program 724, \n{ex . Message}" );
            //        TotalExErrors++;
            //    }
            //    if ( DOSTOP == true )
            //        return totalfiles;

            // We have a set of matching files from the root folder or currently activate subfolder,
            // so now find all the subdirectories under this directory.
            try
            {
                subDirs = root . GetDirectories ( );
                DirectoryCount += subDirs . Length;
                string [ ] AllDirs = StripBlockedFolders ( subDirs , blockdirs );
//                foreach ( DirectoryInfo dirinfo in subDirs )
                    for(int x = 0 ; x< subDirs .Length ;x++ )
                    {
                    DirectoryInfo dirinfo = subDirs [ x ];
                    string dirs = dirinfo . FullName . ToUpper ( );
                    if ( DOSTOP == true ) break;

                    if ( isValidDir ( dirs , AllDirs ) )
                    {
                        //trigger event
                        if ( UpdateDirList != null )
                        {

                            UpdateArgs mea = new UpdateArgs ( );
                            mea . dirname = dirs;
                            UpdateDirList ( this , mea );
                        }
                        //Task task = Task . Run ( ( ) =>
                        //{
                        //Application . Current . Dispatcher . Invoke ( ( ) =>
                        //{
                        MainWindow . panelinfo . Text = dirs;
                        MainWindow . panelinfo . UpdateLayout ( );
                        MainWindow . fileList . Items . Add ( dirs );
                        //} );
                        //} );


                        //UpdateArgs mea = new UpdateArgs ( );
                        //mea . dirname = dirs;
                        // UpdateDirList ( this , mea );
                        //}
                        MainWindow . FulldirectoriesList . Add ( dirs );
                        MainWindow . fileList . UpdateLayout ( );

                        //****************************************************//
                        // Iterate around again
                        int totfiles = 0;
                        WalkDirectoryTree ( dirinfo , filetype , ref totfiles , blockdirs );
                        //****************************************************//
                    }
                }
            }
            catch ( Exception ex )
            { Debug . WriteLine ( $"!!  {ex . Message}" ); }
            return totalfiles;
        }

        private static bool Checkforduplicatefile ( string fname )
        {
            int rootlength = RootPath . Length;
            bool res = false;
            for ( int x = 0 ; x < FullPathFileNameList . Count ; x++ )
            {
                if ( MainWindow . Showfull == true )
                {
                    if ( FullPathFileNameList [ x ] . Trim ( ) . ToUpper ( ) . Contains ( fname . ToUpper ( ) ) == true )
                    {
                        res = true;
                        break;
                    }
                }
                else
                {
                    string balname = fname . Substring ( rootlength , fname . Length - rootlength );
                    if ( FullPathFileNameList [ x ] . Trim ( ) . ToUpper ( ) . Contains ( balname . ToUpper ( ) ) == true )
                    {
                        res = true;
                        break;
                    }
                }
            }
            return res;
        }
        private static bool isValidDir ( string dirs , string [ ] AllDirs )
        {
            foreach ( var item in AllDirs )
            {
                if ( dirs . ToUpper ( ) == item . ToUpper ( ) )
                    return true;
            }
            return false;
        }
        private static string [ ] StripBlockedFolders ( DirectoryInfo [ ] subDirs , string [ ] blockdirs )
        {
            bool doblock = false;
            int count = 0;
            int subdirlength = subDirs . Length;
            string [ ] validdirs = new string [ subdirlength ];
            if ( blockdirs == null || blockdirs . Length == 0 )
            {
                foreach ( var item in subDirs )
                {
                    validdirs [ count++ ] = item . FullName;
                }
                return validdirs;
            }
            validdirs = new string [ subDirs . Length ];
            for ( int x = 0 ; x < subDirs . Length ; x++ )
            {
                string dirs = subDirs [ x ] . FullName . ToUpper ( );
                for ( int y = 0 ; y < blockdirs . Length ; y++ )
                {
                    {
                        if ( dirs . Contains ( blockdirs [ y ] . ToUpper ( ) ) == true )
                        {
                            doblock = true;
                            break;
                        }
                    }
                }
                if ( doblock == false )
                {
                    validdirs [ count++ ] = dirs;
                    doblock = false;
                }
                else doblock = false;
            }
            string [ ] goodirs = new string [ count ];
            int newindx = 0;
            for ( int x = 0 ; x < count ; x++ )
            {
                if ( validdirs [ x ] != null )
                    goodirs [ newindx++ ] = validdirs [ x ];
            }
            // return list of valid subfolders (checked against blockdirs)
            return goodirs;
        }

        private static int AddFilesToList ( string filename , ref int filesindex )
        {
            int counter = 0;
            int rootlength = RootPath . Length;
            if ( Checkforduplicatefile ( filename ) == false )
            {
                AllFileNames [ filesindex++ ] = filename;
                if ( MainWindow . Showfull )
                {
                    FullPathFileNameList . Add ( filename );
                    //Debug . WriteLine ( $"Adding [{FullPathFileNameList . Count}] : {filename} to FullPathFileName<string>" );
                }
                else
                {
                    string balname = "... " + filename . Substring ( rootlength , filename . Length - rootlength );
                    FullPathFileNameList . Add ( balname );
                    //Debug . WriteLine ( $"Adding [{FullPathFileNameList . Count}] : {balname} to FullPathFileName<string>" );
                }
                counter++;
            }
            else
            {
                //Debug . WriteLine ( $"Duplicates file {filename} blocked" );
                duplicates . Add ( filename );
            }
            return counter;
        }
        private int DoIterativeReport ( string fpath )
        {
            DirectoryInfo di = new ( fpath );
            int totfiles = 0;
            int totalfiles = WalkDirectoryTree ( di , validtypes [ 0 ] , ref totfiles , blocktypes );

            //string path1 = $@"{Program . RootPath}\ScanResults2\ AllDirectories.txt";
            //string path2 = $@"{Program . RootPath}\ScanResults2\ AllFiles.txt";
            //if ( FulldirectoriesList != null )
            //{
            //    File . WriteAllText ( path1 , $"{underline}\nCsharpParser.EXE : OUTPUT\nList of ALL ({FulldirectoriesList . Count}) directories checked.\nFile Created : {DateTime . Now . ToString ( )}\n{underline}\n\n" );
            //    foreach ( string item in FulldirectoriesList )
            //    {
            //        File . AppendAllText ( path1 , $"{item}\n" );
            //    }
            //}
            //if ( FullfilesList != null )
            //{
            //    File . WriteAllText ( path2 , $"{underline}\nCsharpParser.EXE : OUTPUT\nList of ({FullfilesList . Count}) files checked. \nFile Created : {DateTime . Now . ToString ( )}\n{underline}\n\n" );
            //    foreach ( string item in FullfilesList )
            //    {
            //        File . AppendAllText ( path2 , $"{item}\n" );
            //    }
            //}
            return totalfiles;
        }
        public static string GetFilenameFromPath ( string path , out string fullpath )
        {
            string output = "";
            fullpath = path;
            if ( path == "" )
                return "";
            try
            {
                string [ ] parts = path . Split ( "\\" );
                output = parts [ parts . Length - 1 ];
                fullpath = "";
                for ( int x = 0 ; x < parts . Length - 1 ; x++ )
                {
                    fullpath += $"{parts [ x ]}\\";
                }
                return output;
            }
            catch ( Exception ex )
            {
                //Add entry to Error Tuple collection 
                string errnsg = $"\n{ex . Message}";
                Tuple<int , string , string> t = Tuple . Create ( -1 , pathfilename , errnsg );
                DebugErrors . Add ( t );
                Debug . WriteLine ( "\nERROR Program 565, \n{ex.Message}" ); TotalExErrors++;
            }
            return output;
        }
        private static void GetStorageType ( string upperline , ref int type )
        {
            if ( upperline . StartsWith ( "PUBLIC" ) || upperline . StartsWith ( "STATIC PUBLIC" ) ) type = 1;
            else if ( upperline . StartsWith ( "PRIVATE" ) || upperline . StartsWith ( "STATIC PRIVATE" ) ) type = 2;
            else if ( upperline . StartsWith ( "INTERNAL" ) || upperline . StartsWith ( "STATIC INTERNAL" ) ) type = 3;
        }
#pragma warning disable CA1822
        // Called iteratively by LoadAllFiles() main calling method
        // receives root directory, a file type from [], blockfolders []
        public int DoLoadAllFiles ( string RootPath , string filetype , string [ ] blockdirs )
        {
            int totfiles = 0, totalfiles = 0;
            DirectoryInfo di = new ( RootPath );
            //Task task = Task . Run ( ( ) =>
            //{
            WalkDirectoryTree ( di , filetype , ref totfiles , blockdirs );
            totalfiles += totfiles;
            if ( totalfiles == 0 )
            {
                return 0;
            }
            return totalfiles;
        }
#pragma warning restore CA1822

        private static string GetMethodBody ( string currentline , string [ ] lines , ref int x )
        {
            // *** This only gets called for Attached properties ***
            // Parse out a block of code lines and return the cleaned
            // up presentation block to caller  to output it.
            int index = x;
            string output = "";
            bool start = true;
            string [ ] args;
            try
            {
                while ( true )
                {
                    if ( start )
                    {
                        // handle first line
                        string [ ] tmpargs = new string [ 1 ];
                        tmpargs [ 0 ] = $"{currentline . TrimStart ( ) . TrimEnd ( )}\n";
                        args = tmpargs [ 0 ] . Split ( "," );
                        for ( int w = 0 ; w < args . Length ; w++ )
                        {
                            if ( tmpargs [ w ] . Contains ( ");" ) )
                                output += $"\t{tmpargs [ w ] . Trim ( )}\n\n";
                            else if ( tmpargs [ w ] . Length > 2 )
                                output += $"{tmpargs [ w ] . Trim ( )}\n";
                        }
                        start = false;
                        index++;
                        continue;
                    }
                    else
                    {
                        args = lines [ index++ ] . Split ( "," );
                    }
                    for ( int w = 0 ; w < args . Length ; w++ )
                    {
                        if ( args [ w ] . Contains ( ");" ) )
                        {
                            output += $"\t{args [ w ] . Trim ( )}\n\n";
                            x = index;
                            return output;
                        }
                        else if ( args [ w ] . Length > 2 )
                            output += $"\t{args [ w ] . Trim ( )}\n";
                    }
                }
            }
            catch ( Exception ex )
            {
                //Add entry to Error Tuple collection 
                string errmsg = $"\n{ex . Message}\n{lines [ x ]}";
                Tuple<int , string , string> t = Tuple . Create ( x , pathfilename , errmsg );
                DebugErrors . Add ( t );
                Debug . WriteLine ( $"oops$ \nERROR Program 664, \n{ex . Message}" ); TotalExErrors++;
            }
            x = index;
            return output;
        }
        public static string [ ] CleanProcLines ( string [ ] proclines )
        {
            // remove all stuff beyond termnation ")" of any procedure
            for ( int y = proclines . Length - 1 ; y >= 0 ; y-- )
            {
                if ( proclines [ y ] . Contains ( ')' ) )
                {
                    proclines [ y ] = proclines [ y ] . Substring ( 0 , proclines [ y ] . IndexOf ( ")" ) + 1 );
                    break;
                }
            }
            return proclines;
        }
    }
}

public class UpdateArgs : EventArgs
{
    public string dirname = "";
}