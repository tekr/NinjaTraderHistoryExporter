NinjaTrader data exporter
===========================
This tool is designed for bulk exporting market data bars from NinjaTrader 7, overcoming the
application limitations that require exporting one instrument at a time.


## Usage
NTDataExporter [-in:<input dir>] -out:<output dir> -sep:<separator> [-l]

  -in:<input dir>   - Directory to read files from. If not specified, will default to
		      "C:\Users\<current user>\NinjaTrader 7\db\minute\"

  -out:<output dir> - Directory to write output to

  -sep:<separator>  - Separator for fields. Defaults to ';' (NinjaTrader export format)
		      if not specified.

  -l 		    - If specified, symbols for input subdirectories ending in xx-xx
		      (where xx-xx is a MM-YY combination) will be converted to a
		      month code (e.g. 06-15 => M5)


## Licence
Permission is hereby granted to distribute, export, modify and otherwise alter this software
in any way, shape, or form as desired. No warranty is granted, either express or implied.
Build and run at your own risk. 