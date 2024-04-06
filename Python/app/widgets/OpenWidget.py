import os, pathlib
from PyQt6.QtWidgets import QWidget, QGridLayout, QFileDialog, QLabel, QComboBox, QLineEdit, QPushButton
from PyQt6.QtCore import Qt
from gamex import families, getFamily, config

familyValues = list(families.values())

# OpenWidget
class OpenWidget(QWidget):
    def __init__(self, app, callback):
        super().__init__()
        self.app = app
        self.callback = callback
        self.result = None
        self.initUI()
        self.show()

    def initUI(self):
        self.setWindowTitle("Open")
        self.setMinimumWidth(300)
        self.setMinimumHeight(200)
        self.resize(400, 200)

        familyLabel = QLabel(self); familyLabel.setText("Family:")
        familyInput = self.familyInput = QComboBox(self)
        familyInput.currentIndexChanged.connect(self.family_change)

        gameLabel = QLabel(self); gameLabel.setText("Game:")
        gameInput = self.gameInput = QComboBox(self)
        gameInput.currentIndexChanged.connect(self.game_change)

        editionLabel = QLabel(self); editionLabel.setText("Edition:")
        editionInput = self.editionInput = QComboBox(self)
        editionInput.currentIndexChanged.connect(self.edition_change)

        pak1uriLabel = QLabel(self); pak1uriLabel.setText("Resource Uri:")
        pak1uriInput = self.pak1uriInput = QLineEdit(self)
        pak1uriButton = QPushButton(self); pak1uriButton.setText("*")
        pak1uriButton.clicked.connect(self.pak1uri_click)

        pak2uriLabel = QLabel(self); pak2uriLabel.setText("Resource Uri:")
        pak2uriInput = self.pak2uriInput = QLineEdit(self)
        pak2uriButton = QPushButton(self); pak2uriButton.setText("*")
        pak2uriButton.clicked.connect(self.pak2uri_click)

        pak3uriLabel = QLabel(self); pak3uriLabel.setText("Resource Uri:")
        pak3uriInput = self.pak3uriInput = QLineEdit(self)
        pak3uriButton = QPushButton(self); pak3uriButton.setText("*")
        pak3uriButton.clicked.connect(self.pak3uri_click)

        cancelButton = QPushButton(self); cancelButton.setText("Cancel")
        cancelButton.clicked.connect(self.cancel_click)
        openButton = QPushButton(self); openButton.setText("Open")
        openButton.clicked.connect(self.open_click)

        layout = QGridLayout()
        layout.addWidget(familyLabel, 0, 0); layout.addWidget(familyInput, 0, 1)
        layout.addWidget(gameLabel, 1, 0); layout.addWidget(gameInput, 1, 1)
        layout.addWidget(editionLabel, 2, 0); layout.addWidget(editionInput, 2, 1)
        layout.addWidget(pak1uriLabel, 3, 0); layout.addWidget(pak1uriInput, 3, 1); layout.addWidget(pak1uriButton, 3, 2)
        layout.addWidget(pak2uriLabel, 4, 0); layout.addWidget(pak2uriInput, 4, 1); layout.addWidget(pak2uriButton, 4, 2)
        layout.addWidget(pak3uriLabel, 5, 0); layout.addWidget(pak3uriInput, 5, 1); layout.addWidget(pak3uriButton, 5, 2)
        layout.addWidget(cancelButton, 6, 1); layout.addWidget(openButton, 6, 2)
        self.setLayout(layout)
        # setup
        self.familyInput.addItems([None] + [x.name for x in familyValues])

    def closeEvent(self, e=None):
        self.app.closeWidget(self)

    @property
    def pakUris(self):
        return [i for i in [self.pak1uriInput.text(), self.pak2uriInput.text(), self.pak3uriInput.text()] if i != '']
    @pakUris.setter
    def pakUris(self, value):
        idx = 0
        pak1uri = None; pak2uri = None; pak3uri = None
        if value:
            for uri in value:
                if not uri: continue
                idx += 1
                match idx:
                    case 1: pak1uri = uri
                    case 2: pak2uri = uri
                    case 3: pak3uri = uri
        self.pak1uriInput.setText(pak1uri)
        self.pak2uriInput.setText(pak2uri)
        self.pak3uriInput.setText(pak3uri)

    def family_change(self, index):
        selected = self.familySelected = familyValues[index - 1] if index > 0 else None
        # related
        self.gameInput.clear()
        if not selected or not selected.games: return
        self.gameValues = list(selected.games.values())
        self.gameInput.addItems([None] + [x.name for x in self.gameValues])

    def game_change(self, index):
        selected = self.gameSelected = self.gameValues[index - 1] if index > 0 else None
        self.pakUris = selected.toPaks(None) if selected else None
        # related
        self.editionInput.clear()
        if not selected or not selected.editions: return
        self.editionValues = list(selected.editions.values())
        self.editionInput.addItems([None] + [x.name for x in self.editionValues])

    def edition_change(self, index):
        selectedGame = self.gameSelected
        selected = self.editionSelected = self.editionValues[index - 1] if index > 0 else None
        self.pakUris = selectedGame.toPaks(selected.id if selected else None) if selectedGame else None

    def pak1uri_click(self):
        openDialog = QFileDialog.getExistingDirectory(self, "Directory", os.getcwd())
        if not openDialog: return None
        fragment = self.gameSelected.id if self.gameSelected else 'Unknown'
        uri = f'{pathlib.Path(openDialog).as_uri()}#{fragment}'
        self.pak1uriInput.setText(uri)
    
    def pak2uri_click(self):
        openDialog = QFileDialog.getExistingDirectory(self, "Directory", os.getcwd())
        if not openDialog: return None
        fragment = self.gameSelected.id if self.gameSelected else 'Unknown'
        uri = f'{pathlib.Path(openDialog).as_uri()}#{fragment}'
        self.pak2uriInput.setText(uri)

    def pak3uri_click(self):
        openDialog = QFileDialog.getExistingDirectory(self, "Directory", os.getcwd())
        if not openDialog: return None
        fragment = self.gameSelected.id if self.gameSelected else 'Unknown'
        uri = f'{pathlib.Path(openDialog).as_uri()}#{fragment}'
        self.pak3uriInput.setText(uri)

    def cancel_click(self):
        self.close()

    def open_click(self):
        self.callback(self)
        self.close()

    def onReady(self):
        if not config.Family: return
        self.familyInput.setCurrentIndex([x.id for x in familyValues].index(config.Family) + 1)
        if not config.Game: return
        self.gameInput.setCurrentIndex([x.id for x in self.gameValues].index(config.Game) + 1)
        if config.Edition:
            self.editionInput.setCurrentIndex([x.id for x in self.editionValues].index(config.Edition) + 1)
        if config.ForceOpen: self.open_click()
