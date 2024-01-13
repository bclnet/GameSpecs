import sys, os
from PyQt6.QtWidgets import QWidget, QTextEdit, QHBoxLayout
from PyQt6.QtGui import QFont
from .util import _pathExtension
# import webbrowser

# HexViewWidget
class HexViewWidget(QWidget):
    def __init__(self, app):
        super().__init__()
        self.app = app
        self.initUI()
        self.rowlen = 0x10
        self.max_hexdump_byte_len = 8192

    def showMedia(self, external_viewer=False):
        c = 0
        self.tmp_file = f'tmp/{c}.{self.ext}'
        if not os.path.exists('tmp'): os.mkdir('tmp')
        while os.path.exists(self.tmp_file):
            c+=1
            self.tmp_file=f'tmp/{c}.{self.ext}'
        with open(self.tmp_file,'wb+') as f:
            f.write(self.content)
        webbrowser.open(os.path.join(os.getcwd(), self.tmp_file))
        self.close()

    def showText(self):
        self.text_edit.setText(str(self.content, self.encoding))
        self.text_edit.show()

    def showHexdump(self):
        t = ''
        hexstrlen = self.rowlen * 3
        charstrlen = self.rowlen
        numbytestoprint = min(self.max_hexdump_byte_len + 1, len(self.content))

        for x in range(0, numbytestoprint, self.rowlen):
            section = self.content[x:x+self.rowlen]
            hexstr = ' '.join([f'{y:02x}' for y in section])
            charstr = ''.join([chr(y) if 0x20 <= y <= 0x7E else '.' for y in section])
            t += f'{x:06x} {hexstr.ljust(hexstrlen)} {charstr.ljust(charstrlen)}\n'

        if numbytestoprint < self.file_size:
            t += f'... ({self.file_size-numbytestoprint} bytes truncated) ...'

        self.text_edit.setText(t)
        self.text_edit.show()

    def getFileInfo(self, path, type, content):
        encoding = 'utf-8'
        ext = _pathExtension(path)[1:]
        if type:
            return (ext, encoding, type)

        import filetype
        g = filetype.guess(content[:4096])
        if g is not None and g.mime.split('/')[0] in ['video', 'audio']:
            type = g.mime.split('/')[0]
            ext = g.extension
        elif (g is None or g.mime.split('/')[0] not in ['application']):
            if content[0:4] == b'\xff\xfe\0\0':
                encoding = 'utf-32'
                type = 'txt'
            elif content[0:2] == b'\xff\xfe':
                encoding = 'utf-16'
                type = 'txt'
            elif all(0x20 <= x <= 0x7E or x in [0xd, 0xa, 0x9] for x in content):
                encoding = 'utf-8'
                type = 'txt'
        return (ext, encoding, type)

    def viewFile(self, filename, content, file_size, file_type=None):
        self.text_edit.setText('Loading your file... Please wait')
        self.content = content
        self.file_size = file_size
        self.ext, self.encoding, self.type = self.getFileInfo(filename, file_type, content)
        
        if self.type == 'txt': # show strings as normal text files
            self.setWindowTitle(f'TextView: Viewing {filename}')
            self.showText()
        elif self.type in ['audio', 'video', 'media']: # play the audio/video externally
            self.setWindowTitle(f'MediaView: Viewing {filename}')
            self.showMedia()
        else: # show binary data in hexview
            self.setWindowTitle(f'HexView: Viewing {filename}')
            self.showHexdump()

        self.content=None # do not need anymore.
        # else:
        #     raise Exception('Unsupported datatype passed to viewFile')

    def initUI(self):
        self.setWindowTitle('FileView: Empty')
        self.setMinimumWidth(625)
        self.setMinimumHeight(200)
        self.resize(625, 300)

        self.text_edit = QTextEdit(self)
        self.text_edit.setText('Close this window if you see this text.')
        self.text_edit.setFont(QFont('Courier New', 10))
        self.text_edit.setReadOnly(True)
        self.text_edit.hide()

        self.tmp_file = None

        self.layout = QHBoxLayout()
        self.layout.addWidget(self.text_edit)
        self.setLayout(self.layout)
        self.show()

    def closeEvent(self, e):
        self.text_edit = None
        self.layout = None
        self.app.closeWidget(self)