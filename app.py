from pathlib import Path
import webview

class Api:
    def quit_game(self):
        self.window.destroy()

if __name__ == '__main__':
    api = Api()
    api.window = webview.create_window('木桩训练场', str(Path(__file__).parent / 'ui' / 'index.html'), js_api=api, width=1100, height=760, min_size=(720, 600))
    webview.start(private_mode=False)
