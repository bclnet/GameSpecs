import sys, os
from PyQt6.QtWidgets import QApplication
from PyQt6.QtCore import Qt
from gamespecs.platform_OpenGL import OpenGLPlatform
from widgets.MainPage import MainPage

if __name__ == '__main__':
    QApplication.setAttribute(Qt.ApplicationAttribute.AA_UseDesktopOpenGL)
    app = QApplication(sys.argv)
    p = MainPage()
    p.onReady()
    sys.exit(app.exec())