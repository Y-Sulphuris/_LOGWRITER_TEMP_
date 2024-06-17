# _LOGWRITER_TEMP_
Инструкция по запуску: через командную строку зайти в [path] (для удобства там есть файл cmd.bat, который запускает командную строку прямо оттуда)

Конфигурация в файле config.properties (в нём же описано, какой проперти за что отвечает)

Команды:
logwriter parse - прорасить все логи и засунуть в базу данных (используется SQLite)
logwriter [filters|tables] - вывести логи в коснсоль из базы данных (если базы данных нет, вызывает parse автоматически)
Фильтры:
-ip - фильтровать по ip
-u - фильтровать по user
-s - фильтровать по статусу
-minDate - минимальная дата
-maxDate - максимальная дата
Формат вводимых дат: "dd.MM.YYYY"

Столбцы для просмотра:
ip, user, name, date, first_line, status, size

Примеры:
logwriter -s 200 - все логи со статусом 200
logwriter date - даты всех логов
logwriter -minDate 25.07.2006 -maxDate 30.07.2006 - все логи в промежутке от 25.07.2006 до 30.07.2006
logwriter -ip 127.0.0.1 status size - статус и размер всех запросов с ip 127.0.0.1

API для json: LogWriter::LogRecord::ToJson (не знаю зачем)
