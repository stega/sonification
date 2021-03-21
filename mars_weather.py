# WEATHER DATA CLASS
# not currently used.

import json

class MarsWeather(object):

  def __init__(self, filename):
    json_data = self.open_file('rems_weather_data.json')
    self.weather_array = []
    self.idx = 0
    for row in json_data:
      day = {
        "min_temp": row["min_temp"],
        "max_temp": row["max_temp"],
        "pressure": row["pressure"],
        "radiation": row["local_uv_irradiance_index"]
      }
      self.weather_array.append(day)

  def open_file(self, filename):
    with open(filename, 'r') as myfile:
        data=myfile.read()
    return json.loads(data)['soles']

  # make MarsWeather iterable
  def __iter__(self):
    return self

  def __next__(self):
    self.idx += 1
    try:
      return self.weather_array[self.idx-1]
    except IndexError:
      self.idx = 0
      raise StopIteration