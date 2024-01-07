import sys, os
import gamespecs.platform_OpenGL
from PyQt6.QtWidgets import QApplication
from PyQt6.QtCore import Qt
from widgets.MainPage import MainPage

if __name__ == '__main__':
    QApplication.setAttribute(Qt.ApplicationAttribute.AA_UseDesktopOpenGL)
    app = QApplication(sys.argv)
    p = MainPage()
    p.onReady()
    sys.exit(app.exec())