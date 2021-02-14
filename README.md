# Battleship

Написать сервис для игры в морской бой:

методы: 

1) /create-matrix (POST)
request: {"range": int} 
создает матрицу для игры в морской бой
range - число, размер поля для игры в морской бой,  если равно 5 то поле будет выглядеть как 
 A B C D E
1
2
3
4
5

2) /ship (POST) 
request: {"Coordinates": string}
Coordinates - это список координат кораблей на этом поле. Выглядит как "1A 2B,3D 3E". Здесь запятыми разделены координаты 1 корабля. 1A 2B = 1A - левый верхний угол корабля, 2B - правый нижний угол корабля, корабли могут быть квадратными, прямоугольными
Если корабли выходят за границы координатной сетки - возвращает 400 ошибку
на матрицу корабли можно поставить только 1 раз, повторное построение матрицы - только после очистки либо завершения предыдущей игры (все корабли утоплены)

3) /shot (POST)
request: {"сoord": string}
сoord - Координаты, по которым был произведен выстрел. Выглядит как "1A", "2A" и так далее
Возвращает структуру
{
	"destroy":bool,
	"knock":bool,
	"end":bool
}

в случае повторного выстрела по тем же координатам - возвращает 400 с сообщением об ошибке
в случае попадания и незатопления корабля - {"destroy":false,"knock":true,"end":false}, при потоплении {"destroy":true,"knock":true,"end":false},
если утоплен последний корабль - {"destroy":true,"knock":true,"end":true}
при выстреле после утопления всех кораблей - вернуть ошибку

4) /clear (POST)
метод очищает предыдущую игру

5) /state (GET)
метод возвращает статистику игры
response:
{
	"ship_count":int, // всего кораблей
	"destroyed" :int, // потоплено
	"knocked"   :int, // подбито
	"shot_count":int  // сделано выстрелов
}