# GraphGame [RU / EN]
Используемая версия Unity: 2021.3.11f1

Приложение создавалось, чтобы сделать процесс обучения Теории графов (в особенности поиска кратчайших путей) немного интереснее.

## Особенности
Главной особенностью является возможность подгрузки своих алгоритмов поиска пути (через .cs или .dll файлы)
прямо во время работы программы без перекомпиляции всего приложения.

### На главном экране имеются
- Перетаскиваемое окно с узлами, где:
  - Узел с Зеленым кружком - начало пути
  - Узел с Красным кружком - конец пути
- Над ним возможность выбрать от какой и до какой ноды будет производиться поиск пути
- Слева от него выбор алгоритма
  - Выпадающее меню, позволяющее выбрать "просто найди кратчайший путь" или "научи, как ты это делаешь" (либо только одну из этих функций, если реализованы не все)
- Над алгоритмами кнопка загрузки кастомных алгоритмов из специальной папки
- В левом нижнем углу кнопка, открывающая окно дебага, позволяющее увидеть сообщения, предупреждения и ошибки, возникающие при компиляции / работе кастомных алгоритмов.

### Главное окно
![main window](/doc_images/main_window.png)

### Окно дебага (слева)
![error tab](/doc_images/error_tab.png)

## Экран режима "обучения"
Заставляет пользователя пошагово пройтись по всем шагам выполнения алгоритма, чтобы понять / закрепить теоретические знания.

### В наличии
- В окне с узелами появились дополнительные обозначения
  - Узел, покрашенный в Зеленый - начало пути текущего шага
  - Узел, покрашенный в Красный - конец пути текущего шага
  - Узел, покрашенный в Желтый - промежуточная точка пути текущего шага
- Над окном с узлами написано, что требуется от пользователя
- Чуть правее - возможность перемотать на нужное кол-во шагов вперед/назад, используя кнопки или слайдер
- Слева от окна с узлами представлена перетаскиваемая (на случай если она будет большой) матрица путей
(сколько единиц стоит перемещение из точки A в точку B по мнению алгоритма на текущем шаге)
- Пользователь может провести линию от одной точки в другую мышкой (ПКМ по узлу очищает все исходящии из него линии)
- Если пользователь нарисовал линиями правильный путь, алгоритм переходит на следующий шаг

![teaching window](/doc_images/teaching.png)

---

# EN version
Unity version used: 2021.3.11f1

This application was created to make the process of learning Graph Theory (especially finding shortest paths) a little more interesting.

## Features
The main feature is the ability to load your own pathfinding algorithms (via .cs or .dll files)
in runtime without recompiling the app.

### The main screen contains
- Draggable nodes window, where:
  - Node with a Green circle - the beginning of the path
  - Node with a Red circle - the end of the path
- Above it, the ability to choose from which and to which node the path search will be performed
- To the left of it there is the algorithm choice tab
  - Drop-down menu that allows you to choose "just start finding shortest path" or "learn me how you do it" (or only one of these functions, if one of them isn't implemented)
- Above the algorithms there is a button that will read and compile custom algorithms from a special folder
- In the lower left corner there is a button that opens debug window allowing you to see messages, warnings and errors that occur during compilation / work of custom algorithms.

### Main window
![main window](/doc_images/main_window.png)

### Debug window (on the left)
![error tab](/doc_images/error_tab.png)

## "Learning" mode window
Forces the user to step through all the algorithm execution steps in order to understand/consolidate theoretical knowledge.

### We have here
- Additional marks appeared in the nodes window
  - Node colored Green - the beginning of the path of the current step
  - Node colored Red - the end of the path of the current step
  - Node colored Yellow - intermediate point of the path of the current step
- Above the nodes window it's written what is required from the user
- To the right of this hint - the ability to rewind to the desired step using buttons or a slider
- To the left of the nodes window there is a draggable (in case it's big) pathfinding matrix
(how many units does it cost to move from point A to point B according to the algorithm at the current step)
- The user can draw a line from one point to another using mouse (RMB clears all lines coming from this node)
- If the user has drawn the correct path with lines, the algorithm proceeds to the next step

![teaching window](/doc_images/teaching.png)
