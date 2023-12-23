import sys, os
from PyQt6.QtWidgets import QMainWindow, QApplication, QWidget, QProgressBar, QTableView, QTableWidget,QTableWidgetItem, QGridLayout, QHeaderView, QAbstractItemView, QLabel, QTextEdit, QHBoxLayout, QMenu, QFileDialog
from PyQt6.QtGui import QIcon, QFont, QDrag, QPixmap, QPainter, QColor, QBrush, QAction
from PyQt6.QtCore import pyqtSlot, Qt, QBuffer, QByteArray, QUrl, QMimeData, pyqtSignal
from PyQt6.QtMultimedia import QMediaPlayer
from PyQt6.QtMultimediaWidgets import QVideoWidget
from PyQt6 import QtCore, QtMultimedia
from widgets.HexViewWidget import HexViewWidget
from widgets.OpenWidget import OpenWidget
from widgets.SaveFileWidget import SaveFileWidget
from gamespecs import Family

class TableFolderItem(object):
    def __init__(self, text, parent, is_folder=False, is_back_button=False, file_data=None):
        self.text = text
        self.is_folder = is_folder
        self.is_back_button = is_back_button
        self.file_data = file_data
        self.parent = parent
        self.color = None
    def get_color(self, force_update=False):
        if self.file_data == None: return None
        if self.color != None and not force_update: return self.color
        self.color = QBrush(QColor(255, 255, 255))
        return self.color
    def __lt__(self, other):
        if self.is_back_button: return True 
        elif other.is_back_button: return False
        elif self.is_folder and not other.is_folder: return True
        elif other.is_folder and not self.is_folder: return False
        else: return self.text < other.text

class FileTableModel(QtCore.QAbstractTableModel):
    def __init__(self, data, parent=None):
        super(FileTableModel, self).__init__(parent)
        self._data = data
    def appendRow(self,w:QTableWidget): self._data.append(w)
    def rowCount(self, parent=None): return len(self._data)
    def columnCount(self, parent=None): return 1
    def sort(self, order, d): self._data = sorted(self._data)
    def data(self, index, role=Qt.ItemDataRole.DisplayRole):
        if role == Qt.ItemDataRole.DisplayRole:
            row = index.row()
            if 0 <= row < self.rowCount():
                return self._data[row].text
        elif role == Qt.ForegroundRole:
            row = index.row()
            if 0 <= row < self.rowCount():
                return self._data[row].get_color()

class FileTableWidget(QTableView):
    def __init__(self,parent):
        self.parent_obj = parent
        super(FileTableWidget, self).__init__(parent)
    def selectionChanged(self, selected, deselected):
        self.parent_obj.on_click(None)
        # print(selected, deselected)

class ExplorerApp(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.title = 'Explorer'
        self.width = 700
        self.height = 500
        self.load_empty_table()
        self.openWidgets = []
        self.pakFiles = []
        self.initUI()

    def load_empty_table(self):
        self.pak=None
        self.files = []
        self.filetree = self.genFileTree()
        self.curPath = []

    def load_dir(self, d):
        self.load_empty_table()
        self.populateTable()
        self.pak = d
        self.files = self.pak.list_files()
        self.filetree = self.genFileTree()
        self.curPath = []
        self.populateTable()

    def genFileTree(self):
        ftree = {'folders':{},'files':{}}
        for f in self.files:
            path = f[0].replace('\\', '/').split('/')
            toptree = ftree
            for sp in path[:-1]:
                if sp not in toptree['folders']:
                    toptree['folders'][sp]={'folders':{},'files':{}}
                toptree = toptree['folders'][sp]
            toptree['files'][path[-1]]=f
        return ftree
        
    def initUI(self):
        self.setWindowTitle(self.title)
        self.resize(self.width, self.height)
        self.createTables()

        # make status    
        self.statusBar = QLabel(self)

        # Add box layout, add table to box layout and add box layout to widget
        self.main_widget = QWidget(self)
        layout = QGridLayout(self.main_widget)
        layout.addWidget(self.fileTable, 0, 0) 
        layout.addWidget(self.infoTable, 0, 1) 
        layout.addWidget(self.statusBar, 1, 0)
        self.main_widget.setLayout(layout)
        self.setCentralWidget(self.main_widget)

        mainMenu = self.menuBar()
        fileMenu = mainMenu.addMenu('&File')
        fileMenu.addAction("&Open", self.openPage_click)

        # Show widget
        self.show()
    
    def populateTable(self):
        curDir = self.filetree
        for x in self.curPath:
            curDir = curDir['folders'][x]
        self.fileTable.setSortingEnabled(False)
        model = self.fileTable.model()
        if model is not None:
            self.fileTable.setModel(None)
            model.deleteLater()
        self.tableModel = FileTableModel([], self.fileTable)
        # for x in range(self.fileTable.rowCount()):
        #     self.fileTable.removeRow(0)
        if self.curPath != []:
            self.tableModel.appendRow(TableFolderItem("..",self,is_folder=True,is_back_button=True))
        for f in curDir['files']:
            self.tableModel.appendRow(TableFolderItem(f,self,file_data=curDir['files'][f]))
        for f in curDir['folders']:
            self.tableModel.appendRow(TableFolderItem("📁"+f,self,is_folder=True))
        self.fileTable.setModel(self.tableModel)
        self.fileTable.setSortingEnabled(True)
        self.fileTable.sortByColumn(0, Qt.SortOrder.AscendingOrder)
        self.fileTable.scrollToTop()

    def createTables(self):
        # Create tables
        self.fileTable = FileTableWidget(self)
        # self.fileTable.setStyleSheet("border:0px")
        self.fileTable.setShowGrid(False)
        # self.fileTable.setColumnCount(1)
        self.fileTable.verticalHeader().hide()
        self.fileTable.setEditTriggers(FileTableWidget.EditTrigger.NoEditTriggers)
        # self.fileTable.setSelectionMode(FileTableWidget.SelectionMode.SingleSelection)
        # self.fileTable.setDragDropMode(FileTableWidget.DragDropMode.DragOnly)
        # self.fileTable.setDragEnabled(True)
        # self.fileTable.setDropIndicatorShown(True)

        header = self.fileTable.horizontalHeader()       
        header.setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        header.hide()
        self.populateTable()        

        self.infoTable = QTableWidget()
        self.infoTable.setColumnCount(1)
        header = self.infoTable.horizontalHeader()       
        header.setSectionResizeMode(0, QHeaderView.ResizeMode.Stretch)
        header.hide()

        self.infoTable.verticalHeader().hide()
        self.infoTable.setShowGrid(False)

        self.infoTable.insertRow(0) # Name
        self.infoTable.setItem(0, 0, QTableWidgetItem(""))
        self.infoTable.insertRow(1) # size
        self.infoTable.setItem(1, 0, QTableWidgetItem(""))
        self.infoTable.insertRow(2)
        self.infoTable.setItem(2, 0, QTableWidgetItem(""))

        self.infoTable.setFixedWidth(200)
        self.infoTable.setSelectionMode(QTableWidget.SelectionMode.NoSelection)
        self.infoTable.setEditTriggers(QTableWidget.EditTrigger.NoEditTriggers)

        # table selection change
        self.fileTable.setContextMenuPolicy(Qt.ContextMenuPolicy.CustomContextMenu)
        self.fileTable.customContextMenuRequested.connect(self.on_right_click)
        self.fileTable.clicked.connect(self.on_click)
        self.fileTable.doubleClicked.connect(self.on_dbl_click)
    
    def keyReleaseEvent(self, e):
        QMainWindow.keyReleaseEvent(self, e)
        if e.key()==Qt.Key_Enter or e.key()==Qt.Key_Return:
            self.on_dbl_click(None)

    def on_right_click(self, QPos=None):
        parent=self.sender()
        pPos=parent.mapToGlobal(QtCore.QPoint(0, 0))
        mPos=pPos+QPos
        self.rcMenu=QMenu(self)
        item_indexs=[x for x in self.fileTable.selectedIndexes()]
        sitems=[self.fileTable.model()._data[x.row()] for x in item_indexs]
        if len(sitems)>1:
            self.rcMenu.addAction('Export files').triggered.connect(lambda:self.save_items(sitems))
        else:
            item=sitems[-1]
            if item.is_folder:
                self.rcMenu.addAction("Open Folder").triggered.connect(lambda:self.on_dbl_click(item_indexs[-1]))
                self.rcMenu.addAction('Export folder').triggered.connect(lambda:self.save_items([item]))
            else:
                self.rcMenu.addAction('View File (Autodetect)').triggered.connect(lambda:self.show_hexview_for_item(item))
                self.rcMenu.addAction('View Hexdump').triggered.connect(lambda:self.show_hexview_for_item(item,"hex"))
                self.rcMenu.addAction('Export file').triggered.connect(lambda:self.save_items([item]))
                self.rcMenu.addAction('Open file outside').triggered.connect(lambda:self.show_hexview_for_item(item,"media"))
        self.rcMenu.move(mPos)
        self.rcMenu.show()

    def save_items(self,items,dest=None):
        curDir = self.filetree
        for x in self.curPath:
            curDir = curDir['folders'][x]
        folder={'folders':{},'files':{}}
        for x in items:
            n=x.text
            if x.is_folder:
                if x.is_back_button: continue
                folder['folders'][n[1:]]=curDir['folders'][n[1:]]
            else:
                folder['files'][n]=curDir['files'][n]
        w = SaveFileWidget(folder, dest, self)
        self.openWidgets.append(w)
    
    def on_click(self, index):
        # row,column = index.row(),index.column()
        item = self.fileTable.selectedIndexes()
        if len(item)<1:
            return
        item=self.fileTable.model()._data[item[0].row()]
        if not item.is_folder:
            self.infoTable.item(0,0).setText("File: "+item.text)
            if self.CASCReader.is_file_fetchable(item.file_data[1],include_cdn=False): 
                # if its fetchable without cdn, it's local. It can't not be fetchable at this 
                #  point since only fetchable files are on the file list.
                self.infoTable.item(2,0).setText(f"DoubleClick to View")
            else:
                self.infoTable.item(2,0).setText(f"DoubleClick to Fetch")

            if self.isCDN:
                finfo = self.CASCReader.get_file_info_by_ckey(item.file_data[1])
                self.infoTable.item(1,0).setText(f"Archive File: {finfo.data_file[:12]} \n {finfo.compressed_size} bytes compressed" if hasattr(finfo,"data_file") else "CDN File")
                # self.infoTable.item(1,0).setText(f"Archive File")
            else:
                size = self.CASCReader.get_file_size_by_ckey(item.file_data[1])
                chunk_count = self.CASCReader.get_chunk_count_by_ckey(item.file_data[1])
                self.infoTable.item(1,0).setText(f"Size: {beautify_filesize(size)} ({chunk_count} chunk(s))")
                self.infoTable.item(2,0).setText(f"")
        else:
            self.infoTable.item(2,0).setText(f"")
            if item.is_back_button: 
                self.infoTable.item(0,0).setText("Parent Directory")
                self.infoTable.item(1,0).setText("")
            else:
                curDir = self.filetree
                for x in self.curPath+[item.text[1:]]:
                    curDir = curDir['folders'][x]
                self.infoTable.item(0,0).setText("Folder: "+item.text[1:])
                self.infoTable.item(1,0).setText(f"Items: {len(curDir['files'])+len(curDir['folders'])}")

    def on_dbl_click(self, index):
        #enter directory, preview if file
        item = self.fileTable.selectedIndexes()
        if len(item)<1:
            return
        item=self.fileTable.model()._data[item[0].row()]
        if item.is_folder:
            if item.is_back_button:
                self.curPath.pop()
            else:
                self.curPath.append(item.text[1:])
            self.populateTable()
        else:
            self.show_hexview_for_item(item)

    def show_hexview_for_item(self,item,force_type=None):
        ckey = item.file_data[1]
        size = self.CASCReader.get_file_size_by_ckey(ckey)
        data = self.CASCReader.get_file_by_ckey(ckey,8*1024) # load 8k
        w = HexViewWidget(self)
        w.viewFile(item.text,data,size,force_type)
        self.openWidgets.append(w)

    def sub_widget_closed(self,w):
        if w in self.openWidgets:
            self.openWidgets.remove(w)
        w.deleteLater()

    def closeEvent(self, e):
        for h in self.openWidgets:
            if isinstance(h, HexViewWidget) and h.tmp_file is not None:
                os.unlink(h.tmp_file)
            h.closeEvent(None)
        self.openWidgets=None
        if os.path.exists("tmp") and len(os.listdir("tmp"))==0:
            os.rmdir("tmp")

    def statusWriteLine(self, value):
        statusBar = self.statusBar
        statusBar.setText(statusBar.text() + value + '\n')

    def onFirstLoad(self):
        self.openPage_click()

    def open(self, family: Family, pakUris: list[str], path: str = None):
        self.pakFiles.clear()
        if not family: return
        for pakUri in pakUris:
            self.statusWriteLine(f"Opening {pakUri}")
            self.pakFiles.append(family.openPakFile(pakUri))
        self.statusWriteLine("Done")
        self.load_dir(self.pakFiles[0])

    def openPage_click(self):
        w = OpenWidget(self, lambda s: (self.open(s.familySelected, OpenWidget.pakUris.__get__(s)) if s.result else None))
        self.openWidgets.append(w)

if __name__ == '__main__':
    app = QApplication(sys.argv)
    ex = ExplorerApp()
    ex.onFirstLoad()
    sys.exit(app.exec())