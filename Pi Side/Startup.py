import time
import string
import Adafruit_GPIO.SPI as SPI
import Adafruit_SSD1306

from PIL import Image
from PIL import ImageDraw
from PIL import ImageFont

import threading
import os
import signal

# Raspberry Pi pin configuration:
RST = 8
DC = 9
SPI_PORT = 0
SPI_DEVICE = 1

disp = Adafruit_SSD1306.SSD1306_128_64(rst=RST, dc=DC, spi=SPI.SpiDev(SPI_PORT,$

# Initialize library.
disp.begin()

image = Image.new('1', (128, 64))
draw = ImageDraw.Draw(image)
draw.ellipse((32, 0, 96, 63), fill=1, outline=1)
disp.image(image)
disp.display()
