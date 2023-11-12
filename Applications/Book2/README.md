
# Making Ebooks

On Fedora (16 and later) you can run something like this::

    $ yum install ruby calibre rubygems ruby-devel rubygem-ruby-debug rubygem-rdiscount
    $ makeebooks en  # will produce a mobi

On MacOS you can do like this:
	
1. INSTALL ruby and rubygems
2. `$ gem install rdiscount`
3. DOWNLOAD Calibre for MacOS and install command line tools. You'll need some dependencies to generate a PDF:
    * pandoc: http://johnmacfarlane.net/pandoc/installing.html
    * xelatex: http://tug.org/mactex/
4. `$ makeebooks zh` #will produce a mobi

On Windows you can do like this:
	
1. Install ruby and related tool from http://rubyinstaller.org/downloads/
    * RubyInstaller (ruby & gem)
    * Development Kit (to build rdiscount gem)
2. Open `cmd` and `$ gem install rdiscount`
3. Install Calibre for Windows from http://calibre-ebook.com/download
4. `$ SET ebook_convert_path=c:\Program Files\Calibre2\ebook-convert.exe`. Modify to suit with your Calibre installed path.
5. Make ebooks:
    * `$ ruby makeebooks vi` #will produce a epub
    * `$ SET FORMAT=mobi` then `$ ruby makeebooks vi` #will produce an mobi
