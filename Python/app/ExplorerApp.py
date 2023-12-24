import sys, os
from PyQt6.QtWidgets import QApplication
from widgets.MainPage import MainPage

if __name__ == '__main__':
    app = QApplication(sys.argv)
    p = MainPage()
    p.onFirstLoad()
    sys.exit(app.exec())