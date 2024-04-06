import sys, os
from PyQt6.QtWidgets import QMainWindow, QApplication, QWidget, QProgressBar, QTableView, QTableWidget, QTableWidgetItem, QGridLayout, QHeaderView, QAbstractItemView, QLabel, QTextEdit, QHBoxLayout, QMenu, QFileDialog, QSplitter, QTabWidget
from PyQt6.QtGui import QIcon, QFont, QDrag, QPixmap, QPainter, QColor, QBrush, QAction
from PyQt6.QtCore import pyqtSlot, Qt, QBuffer, QByteArray, QUrl, QMimeData, pyqtSignal
from PyQt6.QtMultimedia import QMediaPlayer
from PyQt6.QtMultimediaWidgets import QVideoWidget
from PyQt6 import QtCore, QtMultimedia
from widgets.HexViewWidget import HexViewWidget
from widgets.OpenWidget import OpenWidget
from widgets.SaveFileWidget import SaveFileWidget
from gamex import Family, PakFile, util

# https://doc.qt.io/qt-6/qtreeview.html

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
        self.color = QBrush(QColor(0, 0, 0))
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
    def data(self, index, role = Qt.ItemDataRole.DisplayRole):
        if role == Qt.ItemDataRole.DisplayRole:
            row = index.row()
            if 0 <= row < self.rowCount():
                return self._data[row].text
        elif role == Qt.ItemDataRole.ForegroundRole:
            row = index.row()
            if 0 <= row < self.rowCount():
                return self._data[row].get_color()

class FileTableWidget(QTableView):
    def __init__(self, parent):
        self.parent_obj = parent
        super(FileTableWidget, self).__init__(parent)
    def selectionChanged(self, selected, deselected):
        self.parent_obj.on_click(None)
        # print(selected, deselected)

class FileExplorer(QWidget):
    resource = None #ResourceManagerProvider()
    def __init__(self, parent, tab):
        super().__init__()
        self.parent = parent
        self.load_empty_table()
        self.initUI()
        # ready
        pakFile = tab.pakFile
        self.nodes = pakFile.getMetaItems(self.resource)
        self.load_dir(tab.pakFile)

    def load_empty_table(self):
        self.pakFile = None
        self.filetree = self.genFileTree(None)
        self.curPath = []

    def load_dir(self, pakFile):
        self.pakFile = pakFile
        self.filetree = self.genFileTree(self.pakFile)
        self.curPath = []
        self.updateTable()

    def genFileTree(self, pakFile):
        ftree = {'folders':{}, 'files':{}}
        if not pakFile: return ftree
        for f in pakFile.files:
            path = f.path.replace('\\', '/').split('/')
            toptree = ftree
            for sp in path[:-1]:
                if sp not in toptree['folders']:
                    toptree['folders'][sp] = {'folders':{}, 'files':{}}
                toptree = toptree['folders'][sp]
            if f.pak and f.pak.status == PakFile.PakStatus.Opened:
                stree = self.genFileTree(f.pak)
                toptree['folders'][path[-1]] = stree
            else: toptree['files'][path[-1]] = f
        return ftree
        
    def keyReleaseEvent(self, e):
        QMainWindow.keyReleaseEvent(self.parent, e)
        key = e.key()
        if key == Qt.Key.Key_Enter or key == Qt.Key.Key_Return:
            self.on_dbl_click(None)
        if key == Qt.Key.Key_Backspace and self.curPath:
            self.curPath.pop()
            self.updateTable()

    def initUI(self):
        # fileTable
        fileTable = self.fileTable = FileTableWidget(self)
        header = fileTable.horizontalHeader(); header.setSectionResizeMode(QHeaderView.ResizeMode.Stretch); header.hide()
        # fileTable.setStyleSheet('border:0px')
        fileTable.setShowGrid(False)
        fileTable.setMinimumWidth(100)
        fileTable.verticalHeader().hide()
        fileTable.setEditTriggers(FileTableWidget.EditTrigger.NoEditTriggers)
        # fileTable.setSelectionMode(FileTableWidget.SelectionMode.SingleSelection)
        # fileTable.setDragDropMode(FileTableWidget.DragDropMode.DragOnly)
        # fileTable.setDragEnabled(True)
        # fileTable.setDropIndicatorShown(True)
        fileTable.setContextMenuPolicy(Qt.ContextMenuPolicy.CustomContextMenu)
        fileTable.customContextMenuRequested.connect(self.on_right_click)
        fileTable.clicked.connect(self.on_click)
        fileTable.doubleClicked.connect(self.on_dbl_click)
        self.updateTable()

        # infoTable
        infoTable = self.infoTable = QTableWidget()
        infoTable.setColumnCount(1)
        header = infoTable.horizontalHeader(); header.setSectionResizeMode(0, QHeaderView.ResizeMode.Stretch); header.hide()
        infoTable.verticalHeader().hide()
        infoTable.setShowGrid(False)
        infoTable.setSelectionMode(QTableWidget.SelectionMode.NoSelection)
        infoTable.setEditTriggers(QTableWidget.EditTrigger.NoEditTriggers)

        infoTable.insertRow(0); infoTable.setItem(0, 0, QTableWidgetItem('')) # Name
        infoTable.insertRow(1); infoTable.setItem(1, 0, QTableWidgetItem('')) # size
        infoTable.insertRow(2); infoTable.setItem(2, 0, QTableWidgetItem(''))

        # add box layout, add table to box layout and add box layout to widget
        mainWidget = self.mainWidget = QWidget(self)
        layout = QGridLayout(mainWidget)
        layout.addWidget(fileTable, 0, 0)
        layout.addWidget(infoTable, 1, 0)
        mainWidget.setLayout(layout)
        self.show()
    
    def updateTable(self):
        curDir = self.filetree
        for x in self.curPath: curDir = curDir['folders'][x]
        self.fileTable.setSortingEnabled(False)
        model = self.fileTable.model()
        if model is not None:
            self.fileTable.setModel(None)
            model.deleteLater()
        self.tableModel = FileTableModel([], self.fileTable)
        # for x in range(self.fileTable.rowCount()): self.fileTable.removeRow(0)
        if self.curPath != []: self.tableModel.appendRow(TableFolderItem('..', self, is_folder=True, is_back_button=True))
        for f in curDir['files']: self.tableModel.appendRow(TableFolderItem(f, self, file_data = curDir['files'][f]))
        for f in curDir['folders']: self.tableModel.appendRow(TableFolderItem('📁' + f, self, is_folder=True))
        self.fileTable.setModel(self.tableModel)
        self.fileTable.setSortingEnabled(True)
        self.fileTable.sortByColumn(0, Qt.SortOrder.AscendingOrder)
        self.fileTable.scrollToTop()

    def on_right_click(self, QPos=None):
        parent = self.sender()
        pPos = parent.mapToGlobal(QtCore.QPoint(0, 0))
        mPos = pPos + QPos
        self.rcMenu = QMenu(self)
        item_indexs = [x for x in self.fileTable.selectedIndexes()]
        sitems = [self.fileTable.model()._data[x.row()] for x in item_indexs]
        if len(sitems) > 1:
            self.rcMenu.addAction('Export files').triggered.connect(lambda:self.save_items(sitems))
        else:
            item = sitems[-1]
            if item.is_folder:
                self.rcMenu.addAction('Open Folder').triggered.connect(lambda:self.on_dbl_click(item_indexs[-1]))
                self.rcMenu.addAction('Export folder').triggered.connect(lambda:self.save_items([item]))
            else:
                self.rcMenu.addAction('View File (Autodetect)').triggered.connect(lambda:self.show_hexview_for_item(item))
                self.rcMenu.addAction('View Hexdump').triggered.connect(lambda:self.show_hexview_for_item(item, 'hex'))
                self.rcMenu.addAction('Export file').triggered.connect(lambda:self.save_items([item]))
                self.rcMenu.addAction('Open file outside').triggered.connect(lambda:self.show_hexview_for_item(item, 'media'))
        self.rcMenu.move(mPos)
        self.rcMenu.show()

    def save_items(self,items,dest=None):
        curDir = self.filetree
        for x in self.curPath: curDir = curDir['folders'][x]
        folder = {'folders':{}, 'files':{}}
        for x in items:
            n = x.text
            if x.is_folder:
                if x.is_back_button: continue
                folder['folders'][n[1:]] = curDir['folders'][n[1:]]
            else: folder['files'][n] = curDir['files'][n]
        w = SaveFileWidget(folder, dest, self)
        self.openWidgets.append(w)
    
    def on_click(self, index):
        item = self.fileTable.selectedIndexes()
        if len(item) < 1: return
        item = self.fileTable.model()._data[item[0].row()]
        if item.is_folder:
            if item.is_back_button:
                self.infoTable.item(0, 0).setText('Parent Directory')
                self.infoTable.item(1, 0).setText('')
            else:
                curDir = self.filetree
                for x in self.curPath + [item.text[1:]]: curDir = curDir['folders'][x]
                self.infoTable.item(0, 0).setText(f'Folder: {item.text[1:]}')
                self.infoTable.item(1, 0).setText(f"Items: {len(curDir['files'])+len(curDir['folders'])}")
            return
        self.setSelectedItem(item)

    def on_dbl_click(self, index):
        # enter directory, preview if file
        item = self.fileTable.selectedIndexes()
        if len(item) < 1: return
        item = self.fileTable.model()._data[item[0].row()]
        if item.is_folder:
            if item.is_back_button: self.curPath.pop()
            else: self.curPath.append(item.text[1:])
            self.updateTable()
            return
        self.setSelectedItem(item)
        self.show_hexview_for_item(item)

    def setSelectedItem(self, value):
        item = value.file_data
        pak = item.pak
        if pak:
            if pak.status == PakFile.PakStatus.Opened: return
            # open pakfile
            pak.open()
            self.filetree = self.genFileTree(self.pakFile)
            self.updateTable()
        # item
        size = item.fileSize
        self.infoTable.item(0, 0).setText(f'File: {value.text}')
        self.infoTable.item(1, 0).setText(f'Size: {util.grammerSize(size) if size else "n/a"}')

    def show_hexview_for_item(self, item, force_type=None):
        size = item.file_data.fileSize
        data = self.pakFile.loadFileData(item.file_data)
        if not data: return
        w = HexViewWidget(self.parent)
        w.viewFile(item.text, data.read(), size, force_type)
        self.parent.openWidgets.append(w)
