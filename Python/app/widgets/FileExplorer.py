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
from gamespecs import Family, util

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
    def __init__(self):
        super().__init__()
