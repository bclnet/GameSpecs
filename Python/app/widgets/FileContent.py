import sys, os
from PyQt6.QtWidgets import QMainWindow, QApplication, QWidget, QSizePolicy, QProgressBar, QScrollArea, QTableView, QTableWidget, QTableWidgetItem, QGridLayout, QHeaderView, QAbstractItemView, QLabel, QTextEdit, QHBoxLayout, QMenu, QFileDialog, QSplitter, QTabWidget
from PyQt6.QtGui import QIcon, QFont, QDrag, QPixmap, QPainter, QColor, QBrush, QAction
from PyQt6.QtCore import Qt, QBuffer, QByteArray, QUrl, QMimeData, pyqtSignal
from PyQt6.QtMultimedia import QMediaPlayer
from PyQt6.QtMultimediaWidgets import QVideoWidget
from PyQt6 import QtCore, QtMultimedia
from gamex.pak import PakFile
from gamex.meta import MetaContent
from .HexView import HexView
from .TextureView import TextureView

# typedefs
class MetaInfo: pass

# TextView
class TextView(QWidget):
    def __init__(self, parent, tab):
        super().__init__()
        mainWidget = QScrollArea(self)
        mainWidget.setStyleSheet('border:0px;')
        label = QLabel(mainWidget)
        label.setText(tab.value) #.decode('utf8', 'ignore')
        label.setWordWrap(True)
        label.setTextInteractionFlags(Qt.TextInteractionFlag.TextSelectableByMouse)

# NullView
class NullView(QWidget):
    def __init__(self, parent, tab):
        super().__init__()

# FileContent
class FileContent(QTabWidget):
    def __init__(self, parent):
        super().__init__()
        self.parent = parent
        self._graphic = []
        self._contentTabs = []
        self.initUI()

    def initUI(self):
        # self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)
        # self.setStyleSheet('background-color: darkgreen;')
        # content tab
        contentTab = self.contentTab = self #QTabWidget(self)
        # contentTab.setMinimumWidth(300)
        # contentTab.setMinimumHeight(300)

    def updateTabs(self):
        self.contentTab.clear()
        if not self.contentTabs: return
        for tab in self.contentTabs:
            control = TextView(self, tab) if tab.type == 'Text' else \
                HexView(self, tab) if tab.type == 'Hex' else \
                TextureView(self, tab) if tab.type == 'Texture' else \
                NullView(self, tab)
            self.contentTab.addTab(control, tab.name)

    @property
    def graphic(self): return self._graphic
    @graphic.setter
    def graphic(self, value):
        self._graphic = value

    @property
    def contentTabs(self) -> list[MetaContent]: return self._contentTabs
    @contentTabs.setter
    def contentTabs(self, value: list[MetaContent]):
        self._contentTabs = value
        self.updateTabs()

    def onInfo(self, pakFile: PakFile, infos: list[MetaInfo] = None):
        self.graphic = pakFile.graphic
        self.contentTabs = [x.tag for x in infos if isinstance(x.tag, MetaContent)] if infos else None
        self.contentTab.selectedIndex = 0 if infos else -1
