import sys, os
from typing import Any
from PyQt6.QtWidgets import QMainWindow, QApplication, QWidget, QProgressBar, QScrollArea, QTableView, QTableWidget, QTableWidgetItem, QGridLayout, QHeaderView, QAbstractItemView, QLabel, QTextEdit, QVBoxLayout, QHBoxLayout, QMenu, QFileDialog, QSplitter, QTabWidget
from PyQt6.QtGui import QIcon, QFont, QDrag, QPixmap, QPainter, QColor, QBrush, QAction
from PyQt6.QtCore import Qt, QBuffer, QByteArray, QUrl, QMimeData, pyqtSignal
from PyQt6.QtMultimedia import QMediaPlayer
from PyQt6.QtMultimediaWidgets import QVideoWidget
from PyQt6 import QtCore, QtMultimedia
from gamex import Family, config
from .HexViewWidget import HexViewWidget
from .SaveFileWidget import SaveFileWidget
from .OpenWidget import OpenWidget
from .FileContent import FileContent
from .FileExplorer import FileExplorer
from .resourcemgr import ResourceManager

# ExplorerMainTab
class ExplorerMainTab:
    def __init__(self, name: str=None, pakFile: Any=None, appList: list[Any]=None, text: str=None):
        self.name = name
        self.pakFile = pakFile
        self.appList = appList
        self.text = text

# TextBlock
class TextBlock(QWidget):
    def __init__(self, parent, tab):
        super().__init__()
        mainWidget = QScrollArea(self)
        mainWidget.setStyleSheet('border:0px;')
        label = QLabel(mainWidget)
        label.setText(tab.text)
        label.setWordWrap(True)
        label.setTextInteractionFlags(Qt.TextInteractionFlag.TextSelectableByMouse)

# LogBar
class LogBar(QLabel):
    def __init__(self, parent):
        super().__init__(parent)

    def contextMenuEvent(self, e):
        context = QMenu(self)
        clearAction = QAction('Clear', self)
        clearAction.triggered.connect(lambda:self.setText(''))
        quitAction = QAction('Quit', self)
        quitAction.triggered.connect(lambda:exit(0))
        context.addAction(clearAction)
        context.addAction(quitAction)
        context.exec(e.globalPos())

# AppList
class AppList(QWidget):
    def __init__(self, parent, tab):
        super().__init__()

# MainPage
class MainPage(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.resource = ResourceManager()
        self.title = 'Explorer'
        self.width = 800
        self.height = 600
        self.pakFiles = []
        self.openWidgets = []
        self.mainTabs = []
        self.initUI()

    def closeWidget(self, w):
        if w in self.openWidgets: self.openWidgets.remove(w)
        w.deleteLater()

    def closeEvent(self, e):
        for h in self.openWidgets:
            if isinstance(h, HexViewWidget) and h.tmp_file is not None: os.unlink(h.tmp_file)
            h.closeEvent(None)
        self.openWidgets=None
        if os.path.exists('tmp') and len(os.listdir('tmp'))==0: os.rmdir('tmp')

    def initUI(self):
        self.setWindowTitle(self.title)
        self.resize(self.width, self.height)
        
        # main tab
        mainTab = self.mainTab = QTabWidget(self)
        # mainTab.setMinimumWidth(300) # remove
        mainTab.setMaximumWidth(500)
        mainTab.setMaximumWidth(200) # remove
        self.updateTabs()

        # contentBlock
        contentBlock = self.contentBlock = FileContent(self)
        contentBlock.setContentsMargins(50, 50, 50, 50)
        # contentBlock.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)
        # contentBlock.setStyleSheet('background-color: darkgreen;')

        # splitter
        splitter = QSplitter(self)
        splitter.addWidget(mainTab)
        splitter.addWidget(contentBlock)

        # logBar
        logBar = self.logBar = LogBar(self)
        logBar.setAlignment(Qt.AlignmentFlag.AlignTop)
        logBar.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)
        logBar.setStyleSheet('background-color: lightgray;')

        # add to layout
        mainWidget = self.mainWidget = QWidget(self)
        layout = QVBoxLayout(mainWidget)
        layout.addWidget(splitter, 9)
        layout.addWidget(logBar, 1)
        mainWidget.setLayout(layout)
        self.setCentralWidget(mainWidget)

        mainMenu = self.menuBar()
        fileMenu = mainMenu.addMenu('&File')
        fileMenu.addAction('&Open', self.openPage_click)

        # show widget
        self.show()
    
    def updateTabs(self):
        self.mainTab.clear()
        for tab in self.mainTabs:
            control = FileExplorer(self, tab) if tab.pakFile else \
                AppList(self, tab) if tab.appList else \
                TextBlock(self, tab)
            self.mainTab.addTab(control, tab.name)

    def openPage_click(self):
        w = OpenWidget(self, lambda s:self.open(s.familySelected, OpenWidget.pakUris.__get__(s)))
        self.openWidgets.append(w)
        w.onReady()

    def log(self, value):
        logBar = self.logBar
        text = logBar.text()
        logBar.setText(text + value + '\n')

    def open(self, family: Family, pakUris: list[str], path: str = None):
        self.pakFiles.clear()
        if not family: return
        self.familyApps = family.apps
        for pakUri in pakUris:
            self.log(f'Opening {pakUri}')
            pak = family.openPakFile(pakUri)
            if pak: self.pakFiles.append(pak)
        self.log('Done')
        self.onOpened(family, path)

    def onOpened(self, family, path):
        tabs = [ExplorerMainTab(
            name = pakFile.name,
            pakFile = pakFile
        ) for pakFile in self.pakFiles]
        if family.description:
            tabs.append(ExplorerMainTab(
                name = 'Information',
                text = family.description
            ))
        self.mainTabs = tabs
        self.updateTabs()

    def onReady(self):
        if config.ForcePath and config.ForcePath.startswith('app:') and self.familyApps and config.ForcePath[:4] in self.familyApps:
            app = self.familyApps[config.ForcePath[:4]]
        self.openPage_click()
