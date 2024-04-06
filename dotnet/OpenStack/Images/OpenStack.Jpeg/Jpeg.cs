using System.Runtime.InteropServices;
using boolean = System.Byte;
using CLong = System.Int32;
using JDIMENSION = System.UInt32;
using JOCTET = System.Byte;

// Data structures for images (arrays of samples and of DCT coefficients). On 80x86 machines, the image arrays are too big for near pointers, but the pointer arrays can fit in near memory.
using JSAMPLE = System.Byte;
//using JSAMPROW_1 = System.Byte; // ptr to one image row of pixel samples.
using JSAMPARRAY_2 = System.Byte; // ptr to some rows (a 2-D sample array)
using JSAMPIMAGE_3 = System.Byte; // a 3-D sample array: top index is color

using JCOEF = System.Int16;
//using JBLOCKROW_1 = System.NumericsX.JBLOCK;       // pointer to one row of coefficient blocks 
using JBLOCKARRAY_2 = System.NumericsX.JBLOCK; // a 2-D array of coefficient blocks
//using JBLOCKIMAGE_3 = System.NumericsX.JBLOCK; // a 3-D array of coefficient blocks

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)] public unsafe struct JBLOCK { public fixed JCOEF _[Jpeg.DCTSIZE2]; } // one block of coefficients

    public unsafe static partial class Jpeg
    {
        public const int TRUE = 1;
        public const int FALSE = 0;

        internal const string LibraryName = "jpeg9d";

        // Version IDs for the JPEG library. Might be useful for tests like "#if JPEG_LIB_VERSION >= 90".
        internal const int JPEG_LIB_VERSION = 90;   // Compatibility version 9.0
        internal const int JPEG_LIB_VERSION_MAJOR = 9;
        internal const int JPEG_LIB_VERSION_MINOR = 4;

        // Various constants determining the sizes of things. All of these are specified by the JPEG standard, so don't change them if you want to be compatible.
        internal const int DCTSIZE = 8;             // The basic DCT block is 8x8 coefficients
        internal const int DCTSIZE2 = 64;           // DCTSIZE squared; # of elements in a block
        internal const int NUM_QUANT_TBLS = 4;      // Quantization tables are numbered 0..3
        internal const int NUM_HUFF_TBLS = 4;       // Huffman tables are numbered 0..3
        internal const int NUM_ARITH_TBLS = 16;     // Arith-coding tables are numbered 0..15
        internal const int MAX_COMPS_IN_SCAN = 4;   // JPEG limit on # of components in one scan
        internal const int MAX_SAMP_FACTOR = 4;     // JPEG limit on sampling factors

        // Unfortunately, some bozo at Adobe saw no reason to be bound by the standard; the PostScript DCT filter can emit files with many more than 10 blocks/MCU.
        // If you happen to run across such a file, you can up D_MAX_BLOCKS_IN_MCU to handle it.  We even let you do this from the jconfig.h file.  However,
        // we strongly discourage changing C_MAX_BLOCKS_IN_MCU; just because Adobe sometimes emits noncompliant files doesn't mean you should too.
        internal const int C_MAX_BLOCKS_IN_MCU = 10; // compressor's limit on blocks per MCU
        internal const int D_MAX_BLOCKS_IN_MCU = 10; // decompressor's limit on blocks per MCU
    }

    // Types for JPEG compression parameters and working tables.


    // DCT coefficient quantization tables.

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct JQUANT_TBL
    {
        // This array gives the coefficient quantizers in natural array order (not the zigzag order in which they are stored in a JPEG DQT marker).
        // CAUTION: IJG versions prior to v6a kept this array in zigzag order.
        public fixed UInt16 quantval[Jpeg.DCTSIZE2]; // quantization step for each coefficient

        // This field is used only during compression.  It's initialized FALSE when the table is created, and set TRUE when it's been output to the file.
        // You could suppress output of a table by setting this to TRUE. (See jpeg_suppress_tables for an example.)
        public boolean sent_table;      // TRUE when table has been output
    }

    // Huffman coding tables.

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct JHUFF_TBL
    {
        // These two fields directly represent the contents of a JPEG DHT marker
        public fixed Byte bits[17];     // bits[k] = # of symbols with codes of length k bits; bits[0] is unused
        public fixed Byte huffval[256]; // The symbols, in order of incr code length

        // This field is used only during compression.  It's initialized FALSE when the table is created, and set TRUE when it's been output to the file.
        // You could suppress output of a table by setting this to TRUE. (See jpeg_suppress_tables for an example.)
        public boolean sent_table;      // TRUE when table has been output
    }

    // Basic info about one component (color channel).

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_component_info
    {
        // These values are fixed over the whole image. For compression, they must be supplied by parameter setup; for decompression, they are read from the SOF marker.
        public int component_id;        // identifier for this component (0..255)
        public int component_index;     // its index in SOF or cinfo->comp_info[]
        public int h_samp_factor;       // horizontal sampling factor (1..4)
        public int v_samp_factor;       // vertical sampling factor (1..4)
        public int quant_tbl_no;        // quantization table selector (0..3)

        // These values may vary between scans. For compression, they must be supplied by parameter setup;
        // for decompression, they are read from the SOS marker. The decompressor output side may not use these variables.
        public int dc_tbl_no;           // DC entropy table selector (0..3)
        public int ac_tbl_no;           // AC entropy table selector (0..3)

        // Remaining fields should be treated as private by applications.

        // These values are computed during compression or decompression startup: Component's size in DCT blocks.
        // Any dummy blocks added to complete an MCU are not counted; therefore these values do not depend on whether a scan is interleaved or not.
        internal JDIMENSION width_in_blocks;
        internal JDIMENSION height_in_blocks;
        // Size of a DCT block in samples, reflecting any scaling we choose to apply during the DCT step.
        // Values from 1 to 16 are supported. Note that different components may receive different DCT scalings.
        internal int DCT_h_scaled_size;
        internal int DCT_v_scaled_size;
        // The downsampled dimensions are the component's actual, unpadded number of samples at the main buffer (preprocessing/compression interface);
        // DCT scaling is included, so downsampled_width =
        //   ceil(image_width * Hi/Hmax * DCT_h_scaled_size/block_size) and similarly for height.
        internal JDIMENSION downsampled_width;  // actual width in samples
        internal JDIMENSION downsampled_height; // actual height in samples

        // For decompression, in cases where some of the components will be ignored (eg grayscale output from YCbCr image), we can skip most
        // computations for the unused components. For compression, some of the components will need further quantization
        // scale by factor of 2 after DCT (eg BG_YCC output from normal RGB input). The field is first set TRUE for decompression, FALSE for compression
        // in initial_setup, and then adapted in color conversion setup.
        internal boolean component_needed;

        // These values are computed before starting a scan of the component. The decompressor output side may not use these variables.
        internal int MCU_width;         // number of blocks per MCU, horizontally
        internal int MCU_height;        // number of blocks per MCU, vertically
        internal int MCU_blocks;        // MCU_width * MCU_height
        internal int MCU_sample_width;  // MCU width in samples: MCU_width * DCT_h_scaled_size
        internal int last_col_width;    // # of non-dummy blocks across in last MCU
        internal int last_row_height;   // # of non-dummy blocks down in last MCU

        // Saved quantization table for component; NULL if none yet saved. See jdinput.c comments about the need for this information. This field is currently used only for decompression.
        internal JQUANT_TBL* quant_table;

        // Private per-component storage for DCT or IDCT subsystem.
        internal void* dct_table;
    }

    // The script for encoding a multiple-scan file is an array of these:

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_scan_info
    {
        public int comps_in_scan;       // number of components encoded in this scan
        public fixed int component_index[Jpeg.MAX_COMPS_IN_SCAN]; // their SOF/comp_info[] indexes
        public int Ss, Se;              // progressive JPEG spectral selection parms
        public int Ah, Al;              // progressive JPEG successive approx. parms
    }

    // The decompressor can save APPn and COM markers in a list of these:

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_marker_struct
    {
        public jpeg_marker_struct* next; // next in list, or NULL
        public Byte marker;             // marker code: JPEG_COM, or JPEG_APP0+n
        public uint original_length;    // # bytes of data in the file
        public uint data_length;        // # bytes of data saved at data[]
        public JOCTET* data;            // the data contained in the marker

        // the marker length word is not counted in data_length or original_length
    }

    // Known color spaces.
    public enum J_COLOR_SPACE
    {
        JCS_UNKNOWN,        // error/unspecified
        JCS_GRAYSCALE,      // monochrome
        JCS_RGB,            // red/green/blue, standard RGB (sRGB)
        JCS_YCbCr,          // Y/Cb/Cr (also known as YUV), standard YCC
        JCS_CMYK,           // C/M/Y/K
        JCS_YCCK,           // Y/Cb/Cr/K
        JCS_BG_RGB,         // big gamut red/green/blue, bg-sRGB
        JCS_BG_YCC          // big gamut Y/Cb/Cr, bg-sYCC
    }

    // Supported color transforms.
    public enum J_COLOR_TRANSFORM
    {
        JCT_NONE = 0,
        JCT_SUBTRACT_GREEN = 1
    }

    // DCT/IDCT algorithm options.
    public enum J_DCT_METHOD
    {
        JDCT_ISLOW,         // slow but accurate integer algorithm
        JDCT_IFAST,         // faster, less accurate integer method
        JDCT_FLOAT          // floating-point: accurate, fast on fast HW
    }

    partial class Jpeg
    {
        public const J_DCT_METHOD JDCT_DEFAULT = J_DCT_METHOD.JDCT_ISLOW;
        public const J_DCT_METHOD JDCT_FASTEST = J_DCT_METHOD.JDCT_IFAST;
    }

    // Dithering options for decompression.
    public enum J_DITHER_MODE
    {
        JDITHER_NONE,       // no dithering
        JDITHER_ORDERED,    // simple ordered dither
        JDITHER_FS          // Floyd-Steinberg error diffusion dither
    }

    // Common fields between JPEG compression and decompression master structs.

    // Routines that are to be used by both halves of the library are declared to receive a pointer to this structure.  There are no actual instances of
    // jpeg_common_struct, only of jpeg_compress_struct and jpeg_decompress_struct.
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_common_struct
    {
        #region Fields common to both master struct types
        public jpeg_error_mgr* err;             // Error handler module
        public jpeg_memory_mgr* mem;            // Memory manager module
        public jpeg_progress_mgr* progress;     // Progress monitor, or NULL if none
        public void* client_data;               // Available for use by application
        public boolean is_decompressor;         // So common code can tell which is which
        public int global_state;                // For checking call sequence validity
        #endregion
    }

    // Master record for a compression instance

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_compress_struct
    {
        #region Fields common to both master struct types
        public jpeg_error_mgr* err;             // Error handler module
        public jpeg_memory_mgr* mem;            // Memory manager module
        public jpeg_progress_mgr* progress;     // Progress monitor, or NULL if none
        public void* client_data;               // Available for use by application
        public boolean is_decompressor;         // So common code can tell which is which
        public int global_state;                // For checking call sequence validity
        #endregion

        // Destination for compressed data
        public jpeg_destination_mgr* dest;

        // Description of source image --- these fields must be filled in by outer application before starting compression.  in_color_space must
        // be correct before you can even call jpeg_set_defaults().

        public JDIMENSION image_width;          // input image width
        public JDIMENSION image_height;         // input image height
        public int input_components;            // # of color components in input image
        public J_COLOR_SPACE in_color_space;    // colorspace of input image

        public double input_gamma;              // image gamma of input image

        // Compression parameters --- these fields must be set before calling jpeg_start_compress().  We recommend calling jpeg_set_defaults() to
        // initialize everything to reasonable defaults, then changing anything the application specifically wants to change.  That way you won't get
        // burnt when new parameters are added.  Also note that there are several helper routines to simplify changing parameters.

        public uint scale_num, scale_denom;     // fraction by which to scale image

        public JDIMENSION jpeg_width;           // scaled JPEG image width
        public JDIMENSION jpeg_height;          // scaled JPEG image height

        // Dimensions of actual JPEG image that will be written to file, derived from input dimensions by scaling factors above.
        // These fields are computed by jpeg_start_compress(). You can also use jpeg_calc_jpeg_dimensions() to determine these values
        // in advance of calling jpeg_start_compress().

        public int data_precision;              // bits of precision in image data

        public int num_components;              // # of color components in JPEG image
        public J_COLOR_SPACE jpeg_color_space;  // colorspace of JPEG image

        public jpeg_component_info* comp_info;
        // comp_info[i] describes component that appears i'th in SOF

        public JQUANT_TBL* quant_tbl_ptrs0; //: [Jpeg.NUM_QUANT_TBLS]
        public JQUANT_TBL* quant_tbl_ptrs1; //: [Jpeg.NUM_QUANT_TBLS]
        public JQUANT_TBL* quant_tbl_ptrs2; //: [Jpeg.NUM_QUANT_TBLS]
        public JQUANT_TBL* quant_tbl_ptrs3; //: [Jpeg.NUM_QUANT_TBLS]
        public fixed int q_scale_factor[Jpeg.NUM_QUANT_TBLS];
        // ptrs to coefficient quantization tables, or NULL if not defined, and corresponding scale factors (percentage, initialized 100).

        public JHUFF_TBL* dc_huff_tbl_ptrs0; //: [Jpeg.NUM_HUFF_TBLS] 
        public JHUFF_TBL* dc_huff_tbl_ptrs1; //: [Jpeg.NUM_HUFF_TBLS] 
        public JHUFF_TBL* dc_huff_tbl_ptrs2; //: [Jpeg.NUM_HUFF_TBLS] 
        public JHUFF_TBL* dc_huff_tbl_ptrs3; //: [Jpeg.NUM_HUFF_TBLS] 
        public JHUFF_TBL* ac_huff_tbl_ptrs0; //: [Jpeg.NUM_HUFF_TBLS] 
        public JHUFF_TBL* ac_huff_tbl_ptrs1; //: [Jpeg.NUM_HUFF_TBLS] 
        public JHUFF_TBL* ac_huff_tbl_ptrs2; //: [Jpeg.NUM_HUFF_TBLS] 
        public JHUFF_TBL* ac_huff_tbl_ptrs3; //: [Jpeg.NUM_HUFF_TBLS] 
        // ptrs to Huffman coding tables, or NULL if not defined

        public fixed Byte arith_dc_L[Jpeg.NUM_ARITH_TBLS]; // L values for DC arith-coding tables
        public fixed Byte arith_dc_U[Jpeg.NUM_ARITH_TBLS]; // U values for DC arith-coding tables
        public fixed Byte arith_ac_K[Jpeg.NUM_ARITH_TBLS]; // Kx values for AC arith-coding tables

        public int num_scans;                   // # of entries in scan_info array
        public jpeg_scan_info* scan_info;       // script for multi-scan file, or NULL

        // The default value of scan_info is NULL, which causes a single-scan sequential JPEG file to be emitted.  To create a multi-scan file,
        // set num_scans and scan_info to point to an array of scan definitions.

        public boolean raw_data_in;             // TRUE=caller supplies downsampled data
        public boolean arith_code;              // TRUE=arithmetic coding, FALSE=Huffman
        public boolean optimize_coding;         // TRUE=optimize entropy encoding parms
        public boolean CCIR601_sampling;        // TRUE=first samples are cosited
        public boolean do_fancy_downsampling;   // TRUE=apply fancy downsampling
        public int smoothing_factor;            // 1..100, or 0 for no input smoothing
        public J_DCT_METHOD dct_method;         // DCT algorithm selector

        // The restart interval can be specified in absolute MCUs by setting restart_interval, or in MCU rows by setting restart_in_rows
        // (in which case the correct restart_interval will be figured for each scan).
        public uint restart_interval;           // MCUs per restart, or 0 for no restart
        public int restart_in_rows;             // if > 0, MCU rows per restart interval

        // Parameters controlling emission of special markers.

        public boolean write_JFIF_header;       // should a JFIF marker be written?
        public Byte JFIF_major_version;         // What to write for the JFIF version number
        public Byte JFIF_minor_version;
        // These three values are not used by the JPEG code, merely copied into the JFIF APP0 marker.  density_unit can be 0 for unknown,
        // 1 for dots/inch, or 2 for dots/cm.  Note that the pixel aspect ratio is defined by X_density/Y_density even when density_unit=0.
        public Byte density_unit;               // JFIF code for pixel size units
        public UInt16 X_density;                // Horizontal pixel density
        public UInt16 Y_density;                // Vertical pixel density
        public boolean write_Adobe_marker;      // should an Adobe marker be written?

        public J_COLOR_TRANSFORM color_transform;
        // Color transform identifier, writes LSE marker if nonzero

        // State variable: index of next scanline to be written to jpeg_write_scanlines().  Application may use this to control its
        // processing loop, e.g., "while (next_scanline < image_height)".

        public JDIMENSION next_scanline;        // 0 .. image_height-1 

        // Remaining fields are known throughout compressor, but generally should not be touched by a surrounding application.

        // These fields are computed during compression startup
        public boolean progressive_mode;        // TRUE if scan script uses progressive mode
        public int max_h_samp_factor;           // largest h_samp_factor
        public int max_v_samp_factor;           // largest v_samp_factor

        public int min_DCT_h_scaled_size;       // smallest DCT_h_scaled_size of any component
        public int min_DCT_v_scaled_size;       // smallest DCT_v_scaled_size of any component

        public JDIMENSION total_iMCU_rows;      // # of iMCU rows to be input to coef ctlr

        // The coefficient controller receives data in units of MCU rows as defined for fully interleaved scans (whether the JPEG file is interleaved or not).
        // There are v_samp_factor * DCT_v_scaled_size sample rows of each component in an "iMCU" (interleaved MCU) row.

        // These fields are valid during any one scan. They describe the components and MCUs actually appearing in the scan.
        public int comps_in_scan;               // # of JPEG components in this scan
        public jpeg_component_info* cur_comp_info0; //: [Jpeg.MAX_COMPS_IN_SCAN]
        public jpeg_component_info* cur_comp_info1; //: [Jpeg.MAX_COMPS_IN_SCAN]
        public jpeg_component_info* cur_comp_info2; //: [Jpeg.MAX_COMPS_IN_SCAN]
        public jpeg_component_info* cur_comp_info3; //: [Jpeg.MAX_COMPS_IN_SCAN]
        // *cur_comp_info[i] describes component that appears i'th in SOS

        public JDIMENSION MCUs_per_row;         // # of MCUs across the image
        public JDIMENSION MCU_rows_in_scan;     // # of MCU rows in the image

        public int blocks_in_MCU;               // # of DCT blocks per MCU
        public fixed int MCU_membership[Jpeg.C_MAX_BLOCKS_IN_MCU];
        // MCU_membership[i] is index in cur_comp_info of component owning
        // i'th block in an MCU

        public int Ss, Se, Ah, Al;              // progressive JPEG parameters for scan

        public int block_size;                  // the basic DCT block size: 1..16
        public int* natural_order;              // natural-order position array
        public int lim_Se;                      // min( Se, DCTSIZE2-1 )

        // Links to compression subobjects (methods and private variables of modules)
        public jpeg_comp_master* master;
        public jpeg_c_main_controller* main;
        public jpeg_c_prep_controller* prep;
        public jpeg_c_coef_controller* coef;
        public jpeg_marker_writer* marker;
        public jpeg_color_converter* cconvert;
        public jpeg_downsampler* downsample;
        public jpeg_forward_dct* fdct;
        public jpeg_entropy_encoder* entropy;
        public jpeg_scan_info* script_space;    // workspace for jpeg_simple_progression
        public int script_space_size;
    }

    // Master record for a decompression instance
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_decompress_struct
    {
        #region Fields common to both master struct types
        public jpeg_error_mgr* err;             // Error handler module
        public jpeg_memory_mgr* mem;            // Memory manager module
        public jpeg_progress_mgr* progress;     // Progress monitor, or NULL if none
        public void* client_data;               // Available for use by application
        public boolean is_decompressor;         // So common code can tell which is which
        public int global_state;                // For checking call sequence validity
        #endregion

        // Source of compressed data
        public jpeg_source_mgr* src;

        // Basic description of image --- filled in by jpeg_read_header().
        // Application may inspect these values to decide how to process image.

        public JDIMENSION image_width;          // nominal image width (from SOF marker)
        public JDIMENSION image_height;         // nominal image height
        public int num_components;              // # of color components in JPEG image
        public J_COLOR_SPACE jpeg_color_space;  // colorspace of JPEG image

        // Decompression processing parameters --- these fields must be set before calling jpeg_start_decompress().  Note that jpeg_read_header() initializes
        // them to default values.

        public J_COLOR_SPACE out_color_space;   // colorspace for output

        public uint scale_num, scale_denom;     // fraction by which to scale image

        public double output_gamma;             // image gamma wanted in output

        public boolean buffered_image;          // TRUE=multiple output passes
        public boolean raw_data_out;            // TRUE=downsampled data wanted

        public J_DCT_METHOD dct_method;         // IDCT algorithm selector
        public boolean do_fancy_upsampling;     // TRUE=apply fancy upsampling
        public boolean do_block_smoothing;      // TRUE=apply interblock smoothing

        public boolean quantize_colors;         // TRUE=colormapped output wanted

        // the following are ignored if not quantize_colors:
        public J_DITHER_MODE dither_mode;       // type of color dithering to use
        public boolean two_pass_quantize;       // TRUE=use two-pass color quantization
        public int desired_number_of_colors;    // max # colors to use in created colormap

        // these are significant only in buffered-image mode:
        public boolean enable_1pass_quant;      // enable future use of 1-pass quantizer
        public boolean enable_external_quant;   // enable future use of external colormap
        public boolean enable_2pass_quant;      // enable future use of 2-pass quantizer

        // Description of actual output image that will be returned to application. These fields are computed by jpeg_start_decompress().
        // You can also use jpeg_calc_output_dimensions() to determine these values in advance of calling jpeg_start_decompress().
        public JDIMENSION output_width;         // scaled image width
        public JDIMENSION output_height;        // scaled image height
        public int out_color_components;        // # of color components in out_color_space
        public int output_components;           // # of color components returned

        // output_components is 1 (a colormap index) when quantizing colors; otherwise it equals out_color_components.
        public int rec_outbuf_height;           // min recommended height of scanline buffer

        // If the buffer passed to jpeg_read_scanlines() is less than this many rows high, space and time will be wasted due to unnecessary data copying.
        // Usually rec_outbuf_height will be 1 or 2, at most 4.

        // When quantizing colors, the output colormap is described by these fields. The application can supply a colormap by setting colormap non-NULL before
        // calling jpeg_start_decompress; otherwise a colormap is created during jpeg_start_decompress or jpeg_start_output.
        // The map has out_color_components rows and actual_number_of_colors columns.
        public int actual_number_of_colors;     // number of entries in use
        public JSAMPARRAY_2** colormap;         // The color map as a 2-D pixel array

        // State variables: these variables indicate the progress of decompression. The application may examine these but must not modify them.

        // Row index of next scanline to be read from jpeg_read_scanlines(). Application may use this to control its processing loop, e.g.,
        // "while (output_scanline < output_height)".
        public JDIMENSION output_scanline;      // 0 .. output_height-1

        // Current input scan number and number of iMCU rows completed in scan. These indicate the progress of the decompressor input side.
        public int input_scan_number;           // Number of SOS markers seen so far
        public JDIMENSION input_iMCU_row;       // Number of iMCU rows completed

        // The "output scan number" is the notional scan being displayed by the output side.  The decompressor will not allow output scan/row number
        // to get ahead of input scan/row, but it can fall arbitrarily far behind.
        public int output_scan_number;          // Nominal scan number being displayed
        public JDIMENSION output_iMCU_row;      // Number of iMCU rows read

        // Current progression status.  coef_bits[c][i] indicates the precision with which component c's DCT coefficient i (in zigzag order) is known.
        // It is -1 when no data has yet been received, otherwise it is the point transform (shift) value for the most recent scan of the coefficient
        // (thus, 0 at completion of the progression). This pointer is NULL when reading a non-progressive file.

        public IntPtr coef_bits; // -1 or current Al value for each coef -- //: public int (* coef_bits)[Jpeg.DCTSIZE2]; 

        // Internal JPEG parameters --- the application usually need not look at these fields.  Note that the decompressor output side may not use
        // any parameters that can change between scans.

        // Quantization and Huffman tables are carried forward across input datastreams when processing abbreviated JPEG datastreams.

        public JQUANT_TBL* quant_tbl_ptrs0; //: [Jpeg.NUM_QUANT_TBLS]
        public JQUANT_TBL* quant_tbl_ptrs1; //: [Jpeg.NUM_QUANT_TBLS]
        public JQUANT_TBL* quant_tbl_ptrs2; //: [Jpeg.NUM_QUANT_TBLS]
        public JQUANT_TBL* quant_tbl_ptrs3; //: [Jpeg.NUM_QUANT_TBLS]
        // ptrs to coefficient quantization tables, or NULL if not defined

        public JHUFF_TBL* dc_huff_tbl_ptrs0; //: [Jpeg.NUM_HUFF_TBLS]
        public JHUFF_TBL* dc_huff_tbl_ptrs1; //: [Jpeg.NUM_HUFF_TBLS]
        public JHUFF_TBL* dc_huff_tbl_ptrs2; //: [Jpeg.NUM_HUFF_TBLS]
        public JHUFF_TBL* dc_huff_tbl_ptrs3; //: [Jpeg.NUM_HUFF_TBLS]
        public JHUFF_TBL* ac_huff_tbl_ptrs0; //: [Jpeg.NUM_HUFF_TBLS]
        public JHUFF_TBL* ac_huff_tbl_ptrs1; //: [Jpeg.NUM_HUFF_TBLS]
        public JHUFF_TBL* ac_huff_tbl_ptrs2; //: [Jpeg.NUM_HUFF_TBLS]
        public JHUFF_TBL* ac_huff_tbl_ptrs3; //: [Jpeg.NUM_HUFF_TBLS]
        // ptrs to Huffman coding tables, or NULL if not defined

        // These parameters are never carried across datastreams, since they are given in SOF/SOS markers or defined to be reset by SOI.

        public int data_precision;              // bits of precision in image data

        public jpeg_component_info* comp_info;
        // comp_info[i] describes component that appears i'th in SOF

        public boolean is_baseline;             // TRUE if Baseline SOF0 encountered
        public boolean progressive_mode;        // TRUE if SOFn specifies progressive mode
        public boolean arith_code;              // TRUE=arithmetic coding, FALSE=Huffman

        public fixed Byte arith_dc_L[Jpeg.NUM_ARITH_TBLS]; // L values for DC arith-coding tables
        public fixed Byte arith_dc_U[Jpeg.NUM_ARITH_TBLS]; // U values for DC arith-coding tables
        public fixed Byte arith_ac_K[Jpeg.NUM_ARITH_TBLS]; // Kx values for AC arith-coding tables

        public uint restart_interval;           // MCUs per restart interval, or 0 for no restart

        // These fields record data obtained from optional markers recognized by the JPEG library.

        public boolean saw_JFIF_marker;         // TRUE iff a JFIF APP0 marker was found

        // Data copied from JFIF marker; only valid if saw_JFIF_marker is TRUE:
        public Byte JFIF_major_version;         // JFIF version number
        public Byte JFIF_minor_version;
        public Byte density_unit;               // JFIF code for pixel size units
        public UInt16 X_density;                // Horizontal pixel density
        public UInt16 Y_density;                // Vertical pixel density
        public boolean saw_Adobe_marker;        // TRUE iff an Adobe APP14 marker was found
        public Byte Adobe_transform;            // Color transform code from Adobe marker

        public J_COLOR_TRANSFORM color_transform;
        // Color transform identifier derived from LSE marker, otherwise zero

        public boolean CCIR601_sampling;        // TRUE=first samples are cosited

        // Aside from the specific data retained from APPn markers known to the library, the uninterpreted contents of any or all APPn and COM markers
        // can be saved in a list for examination by the application.
        public jpeg_marker_struct* marker_list; // Head of list of saved markers

        // Remaining fields are known throughout decompressor, but generally should not be touched by a surrounding application.

        // These fields are computed during decompression startup
        internal int max_h_samp_factor;         // largest h_samp_factor
        internal int max_v_samp_factor;         // largest v_samp_factor

        internal int min_DCT_h_scaled_size;     // smallest DCT_h_scaled_size of any component
        internal int min_DCT_v_scaled_size;     // smallest DCT_v_scaled_size of any component

        internal JDIMENSION total_iMCU_rows;    // # of iMCU rows in image

        // The coefficient controller's input and output progress is measured in units of "iMCU" (interleaved MCU) rows.  These are the same as MCU rows
        // in fully interleaved JPEG scans, but are used whether the scan is interleaved or not.  We define an iMCU row as v_samp_factor DCT block
        // rows of each component.  Therefore, the IDCT output contains v_samp_factor * DCT_v_scaled_size sample rows of a component per iMCU row.

        internal JSAMPLE* sample_range_limit;   // table for fast range-limiting

        // These fields are valid during any one scan. They describe the components and MCUs actually appearing in the scan.
        // Note that the decompressor output side must not use these fields.

        internal int comps_in_scan;             // # of JPEG components in this scan
        internal jpeg_component_info* cur_comp_info0; //: [Jpeg.MAX_COMPS_IN_SCAN]
        internal jpeg_component_info* cur_comp_info1; //: [Jpeg.MAX_COMPS_IN_SCAN]
        internal jpeg_component_info* cur_comp_info2; //: [Jpeg.MAX_COMPS_IN_SCAN]
        internal jpeg_component_info* cur_comp_info3; //: [Jpeg.MAX_COMPS_IN_SCAN]
        // *cur_comp_info[i] describes component that appears i'th in SOS

        internal JDIMENSION MCUs_per_row;       // # of MCUs across the image
        internal JDIMENSION MCU_rows_in_scan;   // # of MCU rows in the image

        internal int blocks_in_MCU;             // # of DCT blocks per MCU
        internal fixed int MCU_membership[Jpeg.D_MAX_BLOCKS_IN_MCU];
        // MCU_membership[i] is index in cur_comp_info of component owning i'th block in an MCU */

        internal int Ss, Se, Ah, Al;            // progressive JPEG parameters for scan

        // These fields are derived from Se of first SOS marker.
        internal int block_size;                // the basic DCT block size: 1..16
        internal int* natural_order;            // natural-order position array for entropy decode
        internal int lim_Se;                    // min( Se, DCTSIZE2-1 ) for entropy decode

        // This field is shared between entropy decoder and marker parser. It is either zero or the code of a JPEG marker that has been
        // read from the data source, but has not yet been processed.
        internal int unread_marker;

        // Links to decompression subobjects (methods, private variables of modules)
        internal jpeg_decomp_master* master;
        internal jpeg_d_main_controller* main;
        internal jpeg_d_coef_controller* coef;
        internal jpeg_d_post_controller* post;
        internal jpeg_input_controller* inputctl;
        internal jpeg_marker_reader* marker;
        internal jpeg_entropy_decoder* entropy;
        internal jpeg_inverse_dct* idct;
        internal jpeg_upsampler* upsample;
        internal jpeg_color_deconverter* cconvert;
        internal jpeg_color_quantizer* cquantize;
    }

    // "Object" declarations for JPEG modules that may be supplied or called directly by the surrounding application.
    // As with all objects in the JPEG library, these structs only define the publicly visible methods and state variables of a module.  Additional
    // private fields may exist after the public ones.

    // Error handler object
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_error_mgr
    {
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct Union
        {
            [FieldOffset(0)] public fixed int i[8]; //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            [FieldOffset(0)] public fixed char s[JMSG_STR_PARM_MAX]; //[MarshalAs(UnmanagedType.ByValArray, SizeConst = JMSG_STR_PARM_MAX)]
        }

        public const int JMSG_LENGTH_MAX = 200; // recommended size of format_message buffer

        // Error exit handler: does not return to caller
        public delegate* unmanaged<jpeg_common_struct*, void> error_exit;
        // Conditionally emit a trace or warning message
        public delegate* unmanaged<jpeg_common_struct*, int, void> emit_message;
        // Routine that actually outputs a trace or error message
        public delegate* unmanaged<jpeg_common_struct*, void> output_message;
        // Format a message string for the most recent JPEG error or message
        public delegate* unmanaged<jpeg_common_struct*, char*, void> format_message;
        // Reset error state variables at start of a new image 
        public delegate* unmanaged<jpeg_common_struct*, void> reset_error_mgr;

        // The message ID code and any parameters are saved here. A message can have one string parameter or up to 8 int parameters.
        public int msg_code;
        public const int JMSG_STR_PARM_MAX = 80;
        public Union msg_parm;

        // Standard state variables for error facility

        public int trace_level;                 // max msg_level that will be displayed

        // For recoverable corrupt-data errors, we emit a warning message, but keep going unless emit_message chooses to abort.  emit_message
        // should count warnings in num_warnings.  The surrounding application can check for bad data by seeing if num_warnings is nonzero at the
        // end of processing.
        public CLong num_warnings;              // number of corrupt-data warnings

        // These fields point to the table(s) of error message strings. An application can change the table pointer to switch to a different
        // message list (typically, to change the language in which errors are reported).  Some applications may wish to add additional error codes
        // that will be handled by the JPEG library error mechanism; the second table pointer is used for this purpose.
        //
        // First table includes all errors generated by JPEG library itself. Error code 0 is reserved for a "no such error string" message.

        public char** jpeg_message_table;       // Library errors
        public int last_jpeg_message;           // Table contains strings 0..last_jpeg_message

        // Second table can be added by application (see cjpeg/djpeg for example). It contains strings numbered first_addon_message..last_addon_message.
        public char** addon_message_table;      // Non-library errors
        public int first_addon_message;         // code for first string in addon table
        public int last_addon_message;          // code for last string in addon table
    }

    // Progress monitor object
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_progress_mgr
    {
        public delegate* unmanaged<jpeg_common_struct*, void> progress_monitor;

        public CLong pass_counter;              // work units completed in this pass
        public CLong pass_limit;                // total number of work units in this pass
        public int completed_passes;            // passes completed so far
        public int total_passes;                // total number of passes expected
    }

    // Data destination object for compression
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_destination_mgr
    {
        public JOCTET* next_output_byte;        // => next byte to write in buffer
        public nint free_in_buffer;             // # of byte spaces remaining in buffer

        public delegate* unmanaged<jpeg_compress_struct*, void> init_destination;
        public delegate* unmanaged<jpeg_compress_struct*, boolean> empty_output_buffer;
        public delegate* unmanaged<jpeg_compress_struct*, void> term_destination;
    }

    // Data source object for decompression
    public unsafe struct jpeg_source_mgr
    {
        public JOCTET* next_input_byte;         // => next byte to read from buffer
        public nint bytes_in_buffer;            // # of bytes remaining in buffer

        public delegate* unmanaged<jpeg_decompress_struct*, void> init_source;
        public delegate* unmanaged<jpeg_decompress_struct*, boolean> fill_input_buffer;
        public delegate* unmanaged<jpeg_decompress_struct*, CLong, void> skip_input_data;
        public delegate* unmanaged<jpeg_decompress_struct*, int, boolean> resync_to_restart;
        public delegate* unmanaged<jpeg_decompress_struct*, void> term_source;
    }

    // Memory manager object.
    // Allocates "small" objects (a few K total), "large" objects (tens of K), and "really big" objects (virtual arrays with backing store if needed).
    // The memory manager does not allow individual objects to be freed; rather, each created object is assigned to a pool, and whole pools can be freed
    // at once.  This is faster and more convenient than remembering exactly what to free, especially where malloc()/free() are not too speedy.
    // NB: alloc routines never return NULL.  They exit to error_exit if not successful.
    partial class Jpeg
    {
        public const int JPOOL_PERMANENT = 0;  // lasts until master record is destroyed
        public const int JPOOL_IMAGE = 1;  // lasts until done with image/datastream
        public const int JPOOL_NUMPOOLS = 2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct jpeg_memory_mgr
    {
        // Method pointers
        public delegate* unmanaged<jpeg_common_struct*, int, nint, void*> alloc_small;
        public delegate* unmanaged<jpeg_common_struct*, int, nint, void*> alloc_large;
        public delegate* unmanaged<jpeg_common_struct*, int, JDIMENSION, JDIMENSION, JSAMPARRAY_2**> alloc_sarray;
        public delegate* unmanaged<jpeg_common_struct*, int, JDIMENSION, JDIMENSION, JBLOCKARRAY_2**> alloc_barray;
        public delegate* unmanaged<jpeg_common_struct*, int, boolean, JDIMENSION, JDIMENSION, JDIMENSION, jvirt_sarray_control*> request_virt_sarray;
        public delegate* unmanaged<jpeg_common_struct*, int, boolean, JDIMENSION, JDIMENSION, JDIMENSION, jvirt_barray_control*> request_virt_barray;
        public delegate* unmanaged<jpeg_common_struct*, void> realize_virt_arrays;
        public delegate* unmanaged<jpeg_common_struct*, jvirt_sarray_control*, JDIMENSION, JDIMENSION, boolean, JSAMPARRAY_2**> access_virt_sarray;
        public delegate* unmanaged<jpeg_common_struct*, jvirt_barray_control*, JDIMENSION, JDIMENSION, boolean, JBLOCKARRAY_2**> access_virt_barray;
        public delegate* unmanaged<jpeg_common_struct*, int, void> free_pool;
        public delegate* unmanaged<jpeg_common_struct*, void> self_destruct;

        // Limit on memory allocation for this JPEG object.  (Note that this is merely advisory, not a guaranteed maximum; it only affects the space
        // used for virtual-array buffers.)  May be changed by outer application after creating the JPEG object.
        public CLong max_memory_to_use;

        // Maximum allocation request accepted by alloc_large.
        public CLong max_alloc_chunk;
    }

    unsafe partial class Jpeg
    {
        // Default error-management setup
        [DllImport(LibraryName, ExactSpelling = true)] public static extern jpeg_error_mgr* jpeg_std_error(jpeg_error_mgr* err);

        // Initialization of JPEG compression objects.
        // jpeg_create_compress() and jpeg_create_decompress() are the exported names that applications should call.  These expand to calls on
        // jpeg_CreateCompress and jpeg_CreateDecompress with additional information passed for version mismatch checking.
        // NB: you must set up the error-manager BEFORE calling jpeg_create_xxx.
        public static void jpeg_create_compress(jpeg_compress_struct* cinfo) => jpeg_CreateCompress(cinfo, JPEG_LIB_VERSION, sizeof(jpeg_compress_struct));
        public static void jpeg_create_decompress(jpeg_decompress_struct* cinfo) => jpeg_CreateDecompress(cinfo, JPEG_LIB_VERSION, sizeof(jpeg_decompress_struct));
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_CreateCompress(jpeg_compress_struct* cinfo, int version, nint structsize);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_CreateDecompress(jpeg_decompress_struct* cinfo, int version, nint structsize);
        // Destruction of JPEG compression objects
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_destroy_compress(jpeg_compress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_destroy_decompress(jpeg_decompress_struct* cinfo);

        // Standard data source and destination managers: stdio streams. Caller is responsible for opening the file before and closing after.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_stdio_dest(jpeg_compress_struct* cinfo, void* outfile);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_stdio_src(jpeg_decompress_struct* cinfo, void* infile);

        // Data source and destination managers: memory buffers.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_mem_dest(jpeg_compress_struct* cinfo, byte** outbuffer, nint* outsize);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_mem_src(jpeg_decompress_struct* cinfo, byte* inbuffer, nint insize);

        // Default parameter setup for compression
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_set_defaults(jpeg_compress_struct* cinfo);
        // Compression parameter setup aids
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_set_colorspace(jpeg_compress_struct* cinfo, J_COLOR_SPACE colorspace);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_default_colorspace(jpeg_compress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_set_quality(jpeg_compress_struct* cinfo, int quality, boolean force_baseline);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_set_linear_quality(jpeg_compress_struct* cinfo, int scale_factor, boolean force_baseline);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_default_qtables(jpeg_compress_struct* cinfo, boolean force_baseline);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_add_quant_table(jpeg_compress_struct* cinfo, int which_tbl, uint* basic_table, int scale_factor, boolean force_baseline);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int jpeg_quality_scaling(int quality);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_simple_progression(jpeg_compress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_suppress_tables(jpeg_compress_struct* cinfo, boolean suppress);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern JQUANT_TBL* jpeg_alloc_quant_table(jpeg_common_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern JHUFF_TBL* jpeg_alloc_huff_table(jpeg_common_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern JHUFF_TBL* jpeg_std_huff_table(jpeg_common_struct* cinfo, boolean isDC, int tblno);

        // Main entry points for compression
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_start_compress(jpeg_compress_struct* cinfo, boolean write_all_tables);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern JDIMENSION jpeg_write_scanlines(jpeg_compress_struct* cinfo, JSAMPARRAY_2** scanlines, JDIMENSION num_lines);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_finish_compress(jpeg_compress_struct* cinfo);

        // Precalculate JPEG dimensions for current compression parameters.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_calc_jpeg_dimensions(jpeg_compress_struct* cinfo);

        // Replaces jpeg_write_scanlines when writing raw downsampled data.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern JDIMENSION jpeg_write_raw_data(jpeg_compress_struct* cinfo, JSAMPIMAGE_3*** data, JDIMENSION num_lines);

        // Write a special marker.  See libjpeg.txt concerning safe usage.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_write_marker(jpeg_compress_struct* cinfo, int marker, JOCTET* dataptr, uint datalen);
        // Same, but piecemeal.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_write_m_header(jpeg_compress_struct* cinfo, int marker, uint datalen);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_write_m_byte(jpeg_compress_struct* cinfo, int val);

        // Alternate compression function: just write an abbreviated table file
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_write_tables(jpeg_compress_struct* cinfo);

        // Decompression startup: read start of JPEG datastream to see what's there
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int jpeg_read_header(jpeg_decompress_struct* cinfo, boolean require_image);
        // Return value is one of:
        public const int JPEG_SUSPENDED = 0; // Suspended due to lack of input data
        public const int JPEG_HEADER_OK = 1; // Found valid image datastream
        public const int JPEG_HEADER_TABLES_ONLY = 2; // Found valid table-specs-only datastream

        // If you pass require_image = TRUE (normal case), you need not check for a TABLES_ONLY return code; an abbreviated file will cause an error exit.
        // JPEG_SUSPENDED is only possible if you use a data source module that can give a suspension return (the stdio source module doesn't).

        // Main entry points for decompression 
        [DllImport(LibraryName, ExactSpelling = true)] public static extern boolean jpeg_start_decompress(jpeg_decompress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern JDIMENSION jpeg_read_scanlines(jpeg_decompress_struct* cinfo, JSAMPARRAY_2** scanlines, JDIMENSION max_lines);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern boolean jpeg_finish_decompress(jpeg_decompress_struct* cinfo);

        // Replaces jpeg_read_scanlines when reading raw downsampled data.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern JDIMENSION jpeg_read_raw_data(jpeg_decompress_struct* cinfo, JSAMPIMAGE_3*** data, JDIMENSION max_lines);

        // Additional entry points for buffered-image mode.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern boolean jpeg_has_multiple_scans(jpeg_decompress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern boolean jpeg_start_output(jpeg_decompress_struct* cinfo, int scan_number);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern boolean jpeg_finish_output(jpeg_decompress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern boolean jpeg_input_complete(jpeg_decompress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_new_colormap(jpeg_decompress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int jpeg_consume_input(jpeg_decompress_struct* cinfo);
        // Return value is one of:
        // public const int JPEG_SUSPENDED	0;    Suspended due to lack of input data
        public const int JPEG_REACHED_SOS = 1; // Reached start of new scan
        public const int JPEG_REACHED_EOI = 2; // Reached end of image
        public const int JPEG_ROW_COMPLETED = 3; // Completed one iMCU row
        public const int JPEG_SCAN_COMPLETED = 4; // Completed last iMCU row of a scan

        // Precalculate output dimensions for current decompression parameters.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_core_output_dimensions(jpeg_decompress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_calc_output_dimensions(jpeg_decompress_struct* cinfo);

        // Control saving of COM and APPn markers into marker_list.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_save_markers(jpeg_decompress_struct* cinfo, int marker_code, uint length_limit);

        // Install a special processing method for COM or APPn markers.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_set_marker_processor(jpeg_decompress_struct* cinfo, int marker_code, delegate* unmanaged<jpeg_decompress_struct*, boolean> routine);

        // Read or write raw DCT coefficients --- useful for lossless transcoding.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern jvirt_barray_control** jpeg_read_coefficients(jpeg_decompress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_write_coefficients(jpeg_compress_struct* cinfo, jvirt_barray_control** coef_arrays);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_copy_critical_parameters(jpeg_decompress_struct* srcinfo, jpeg_compress_struct* dstinfo);

        // If you choose to abort compression or decompression before completing jpeg_finish_(de)compress, then you need to clean up to release memory,
        // temporary files, etc.  You can just call jpeg_destroy_(de)compress if you're done with the JPEG object, but if you want to clean it up and
        // reuse it, call this:
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_abort_compress(jpeg_compress_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_abort_decompress(jpeg_decompress_struct* cinfo);

        // Generic versions of jpeg_abort and jpeg_destroy that work on either flavor of JPEG object.  These may be more convenient in some places.
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_abort(jpeg_common_struct* cinfo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void jpeg_destroy(jpeg_common_struct* cinfo);

        // Default restart-marker-resync procedure for use by data source modules
        [DllImport(LibraryName, ExactSpelling = true)] public static extern boolean jpeg_resync_to_restart(jpeg_decompress_struct* cinfo, int desired);

        // These marker codes are exported since applications and data source modules
        public const byte JPEG_RST0 = 0xD0;  // RST0 marker code
        public const byte JPEG_EOI = 0xD9; // EOI marker code
        public const byte JPEG_APP0 = 0xE0;  // APP0 marker code
        public const byte JPEG_COM = 0xFE;  // COM marker code
    }

    #region INCOMPLETE_TYPES_BROKEN

    public struct jvirt_sarray_control { int dummy; };
    public struct jvirt_barray_control { int dummy; };
    public struct jpeg_comp_master { int dummy; };
    public struct jpeg_c_main_controller { int dummy; };
    public struct jpeg_c_prep_controller { int dummy; };
    public struct jpeg_c_coef_controller { int dummy; };
    public struct jpeg_marker_writer { int dummy; };
    public struct jpeg_color_converter { int dummy; };
    public struct jpeg_downsampler { int dummy; };
    public struct jpeg_forward_dct { int dummy; };
    public struct jpeg_entropy_encoder { int dummy; };
    public struct jpeg_decomp_master { int dummy; };
    public struct jpeg_d_main_controller { int dummy; };
    public struct jpeg_d_coef_controller { int dummy; };
    public struct jpeg_d_post_controller { int dummy; };
    public struct jpeg_input_controller { int dummy; };
    public struct jpeg_marker_reader { int dummy; };
    public struct jpeg_entropy_decoder { int dummy; };
    public struct jpeg_inverse_dct { int dummy; };
    public struct jpeg_upsampler { int dummy; };
    public struct jpeg_color_deconverter { int dummy; };
    public struct jpeg_color_quantizer { int dummy; };

    #endregion
}