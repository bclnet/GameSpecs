# Game Specs: An Asset Guide for Digital Creators, First Edition

Welcome to the first edition of Game Specs: An Asset Guide for Digital Creators.

Game Specs: An Asset Guide for Digital Creators is open source under the MIT license.


## How To Generate the Book

You can generate the e-book files manually with Asciidoctor.
If you run the following you _may_ actually get HTML, Epub, Mobi and PDF output files:

```
$ bundle install
$ bundle exec rake book:build
Converting to HTML...
 -- HTML output at progit.html
Converting to EPub...
 -- Epub output at progit.epub
Converting to Mobi (kf8)...
 -- Mobi output at progit.mobi
Converting to PDF...
 -- PDF output at progit.pdf
```

You can generate just one of the supported formats (HTML, EPUB, mobi, or PDF).
Use one of the following commands:

To generate the HTML book:

```
$ bundle exec rake book:build_html
```

To generate the EPUB book:

```
$ bundle exec rake book:build_epub
```

To generate the mobi book:

```
$ bundle exec rake book:build_mobi
```

To generate the PDF book:

```
$ bundle exec rake book:build_pdf
```

pip install protobuf
pip install qrcode
pip install psutil
