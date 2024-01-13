import os
from PyQt6.QtWidgets import QWidget, QProgressBar, QGridLayout, QFileDialog, QLabel
from PyQt6.QtCore import pyqtSlot, pyqtSignal, Qt
from time import time

# SaveFileWidget
class SaveFileWidget(QWidget):
    TransferCompletedSignal = pyqtSignal(int)

    def __init__(self, items, dest, app): 
        super().__init__()
        assert 'folders' in items and 'files' in items
        self.items = items
        self.app = app
        self.is_single_file = len(self.items['folders']) == 0 and len(self.items['files']) == 1 # no folders, one file.

        if dest is not None:
            assert isinstance(dest, str)
            assert dest is None or (self.is_single_file and os.path.basename(dest)=='') or (not self.is_single_file and os.path.basename(dest)!='')
        self.dest = dest

        self.initUI()
        self.prepare_transfer()
        self.show()

        from threading import Thread
        self.stopped=False
        self.transferThread = Thread(target=self.begin_transfer)
        self.transferThread.start()

    def initUI(self):
        self.setWindowTitle("File Exporter")
        self.setMinimumWidth(300)
        self.setMinimumHeight(100)
        self.resize(300, 100)

        self.progressbar = QProgressBar(self)
        self.leftlabel = QLabel(self)
        self.rightlabel = QLabel(self)
        self.rightlabel.setAlignment(Qt.AlignRight | Qt.AlignVCenter)

        self.layout = QGridLayout()
        self.layout.addWidget(self.progressbar, 2, 1)
        self.layout.addWidget(self.leftlabel, 1, 1)
        self.layout.addWidget(self.rightlabel, 1, 1)
        self.setLayout(self.layout)

        self.TransferCompletedSignal.connect(self.update_progbar)

    def prepare_transfer(self):
        #Verify / Get destination
        self.items_to_save = self.get_items_in_folder(self.items)
        self.completion_count = 0
        self.finishedBytes = 0
        self.to_complete=len(self.items_to_save)
        self.progressbar.setMinimum(0)
        self.progressbar.setMaximum(self.to_complete)
        self.start_time = time()
        self.total_size = 0

        self.update_progbar(0)

        if self.dest is None:
            if self.is_single_file:
                self.dest = QFileDialog.getSaveFileName(self, f"Export {self.items_to_save[0][0]} to...?", os.path.join(os.getcwd(),self.items_to_save[0][0]))[0]
            else:
                self.dest = QFileDialog.getExistingDirectory(self, f"Export {self.to_complete} files to?", os.getcwd())
        

        # if it's not none, we've already made our assertions in the init function, either way we're ready to go.

    def update_progbar(self, filesize:int):
        new_val = self.progressbar.value()+1
        if new_val >= self.progressbar.maximum():
            self.progressbar.setValue(self.progressbar.maximum())
        else:
            self.progressbar.setValue(new_val)
        self.completion_count = new_val
        self.finishedBytes += filesize
        self.transferspeed = round(self.finishedBytes/(time()-self.start_time+0.01),2)
        self.update_labels()
    
    def update_labels(self):
        self.leftlabel.setText(f"{os.path.basename(self.items_to_save[self.completion_count-1][0])}\nFile {self.completion_count} of {self.to_complete}\n\n")
        # self.rightlabel.setText(f"\n{beautify_filesize(self.finishedBytes)}/{beautify_filesize(self.total_size)}\n{beautify_filesize(self.transferspeed)}/s\n")

    def clean_path(self,path):
        validchars = '-_.()\\/ abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'
        return ''.join(c if c in validchars else "_" for c in path)

    def get_items_in_folder(self,folder,curdir=""):
        items=[]
        for fk in folder['folders']:
            f=folder['folders'][fk]
            items.extend(self.get_items_in_folder(f,os.path.join(curdir,self.clean_path(fk))))
        for f in folder['files']:
            file_size = self.cascviewapp.CASCReader.get_file_size_by_ckey(folder['files'][f][1])
            items.append((os.path.join(curdir,self.clean_path(f)),folder['files'][f][1],file_size))
        return items

    def begin_transfer(self):
        self.total_size = sum(x[2] for x in self.items_to_save)
        for x in self.items_to_save:
            if self.stopped: break
            self.save_by_ckey(self.dest if self.is_single_file else os.path.join(self.dest,x[0]),x[1])

    def save_by_ckey(self, path, ckey):
        os.makedirs(os.path.dirname(path),exist_ok=True)
        data=self.cascviewapp.CASCReader.get_file_by_ckey(ckey)
        with open(path,'wb+') as f:
            f.write(data)
        self.TransferCompletedSignal.emit(len(data))

    def closeEvent(self, e):
        self.stopped=True
        self.app.closeWidget(self)