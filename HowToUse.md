# 使い方

## サンプルプロジェクトの使い方

### コードを書いて実行までの流れ
ゲームを開始したら左側のInputFieldにWapL(私の自作言語です)でコードを書く<br>
(InputFieldには表示できる文字数/行数に限界があるため他のテキストエディタでコードを書いてそれをCopy & Pasteするのをおすすめします.)
<br>
↓
<br>
**R + F5**でコードをインタプリタに読み込ませる<br>
ここで疑似メモリのすべてを解放,ストップウォッチ,変数,関数,コードの位置を表すラベルをすべて削除&初期化
<br>
↓
<br>
**D + F5**でコードを1回実行<br>
**D + F + F5**でコードを毎フレーム実行<br>
**D + F + F4**で毎フレーム実行しているのを停止<br>
### その他操作

**D + Escape**でコードの入力/出力画面を閉じる<br>
**R + Escape**でコードの入力/出力画面を開く
<br>

## コードの文法,標準で使える関数

### 第1章:変数の宣言,代入

#### 代入,値を変化

```wapl
=(a,10,i32);
```
でaという名前の変数に10という値をi32(32bit整数)という型で代入します.<br>
```wapl
=(a,1,i32);
+=(a,1);
print(a);
```
これを実行すると**2**が出力されます.<br>

#### 同じ名前で変数を作ったときの挙動

```wapl
=(a,1,i32);
print(ptr(a)); 
print(type(a));
print(a);
=(a,"hoge",String);
print(ptr(a));
print(type(a));
print(a);
```
ptr(変数名)でポインタを得られます.詳しくはメモリ管理の章で説明します.<br>type()はその値の型を返します<br>
このコードを実行すると
```
0
i32
1
0
String
hoge
```
のように出力されます.<br>
print(ptr(a)) は**a**のポインタなので**0**とは限りませんが一度目も二度目も同じ値が出力されます.つまり,同じアドレスに代入され,型や値はあとに代入されたものになります.<br>
#### 型の種類

整数型: i32, i64<br>
浮動小数点数型: f32, f64<br>
文字列型: String<br>
真偽値型: bool<br>
配列: vec<br>
ポインタ型: ptr<br>
GameObject: gob　//Unity版のみ<br>
Vector3: vec3　//Unity版のみ<br>

#### 数値計算

```wapl
//足し算;
=(sum,+(5,10),i32);

//引き算;
=(difference,-(95.5,4.3),f32);

//掛け算;
=(product,*(4,30),i32);

//割り算;
=(quotient,/(56.7,32.2),f64);

//余り;
=(remainder,%(10,4),i32);
```

#### 真偽値

```wapl
=(t,true,bool);
=(f,false,bool);

=(and,and(t,f),bool);
=(and,&&(t,f),bool);

=(or,or(t,f),bool);
=(or,||(t,f),bool);

=(not,not(t),bool);
=(not,!(f),bool);

```

#### 文字列

```wapl
//文字列の結合;
=(str,t+("hello","world"),String);
```

#### 配列
```wapl
//値をvec()でvec化
=(v,vec(1,2,3,"hoge",true),vec);

//0からで2番目の要素;
=(a,get_at(v,2),i32); //3;

//vecの要素数;
=(length,len(v),i32); //5;

//capacityを指定して定義;
=(v2,vec(),vec_2);

//要素の追加;
push(v2,1);
push(v2,2);

//メモリを解放せずにvecの中身をなくす;
clear(v);

//同じ中身を参照する配列を作る;
=(v3,vec(1,2,3),vec);
=(v4,v3,vec);

//中身をコピー;
=(v5,vec(1,2,3),vec);
=(v6,expand(v5),vec);

//0個目の要素のポインタ;
=(pointer,vec_start(v5),ptr);
```

### 第2章:関数,制御フロー

#### 関数

```wapl
main();

fn main(){;
    print("Hello main");
    another_function();
};

fn another_function(){;
    print("AnotherFunction");
};

```
**fn**キーワードで始まり,丸かっこ内に引数,波かっこ内に処理を書きます.<br>
波かっこのあとのセミコロンを忘れないように注意してください.

#### 引数,戻り値を持つ関数

```wapl
print(add(1,2));

fn add(i32 x,i32 y){;
    return +(x,y);
};

```
","区切りで 型 変数名の順で半角スペースを空けて書きます.

#### スコープ

```wapl
fn addOne(i32 b){;
    +=(b,1);
    print(a); //2;
    print(b); //3;
};

=(a,2,i32);
addOne(a);
print(b); //b bという変数は存在しないので文字列として"b"と解釈されて表示される;
```
**a**はどの関数の中のスコープにも入ってないところで定義されているためどこからでもアクセスできます.<br>
一方,**b**はaddOneのスコープにあるので他のところからアクセスすることはできません.<br>
```wapl
fn addOne(i32 b){;
    +=(b,1);
    =(c,b,gbl_i32); //gbl_属性をつけて定義;
    print(a); //2;
    print(b); //3;
};

=(a,2,i32);
addOne(a);
print(c);//3;
```
関数のなかで定義された変数でも,型の初めに**gbl_**とgbl属性をつけることでどこからでもアクセスできるようになります.<br>

#### 関数に参照渡し

```wapl
fn addOne(i32 &_b){;
    +=(&_b,1);
};

=(a,2,i32);
addOne(ptr(a));
print(a); //3;
```
参照渡しをするときは引数の名前を&_から始め,引数として渡す値はポインタを渡します.

#### 配列のメモリ解放

```wapl
main();
fn main(){;
    =(v,vec(1,2,3),vec);
    free(v);
    return 0;
};
```
基本的に関数がreturnをするときにそのスコープの変数の占有していたメモリは自動で解放されますが,疑似メモリ上でvecは{始まりのポインタ,len,capacity}の情報のみを持ち,中身は自動では解放されません.そのため,free()で明示的に解放する必要があります.

#### ラベルにワープ

```wapl
print(1);
warpto(A);
print(2); //これは呼ばれない;
point A;
print(3);
```
**point ラベル名**でラベルをつけて,**warpto**でそのラベルに飛びます.

#### 条件分岐のあるワープ
```wapl
=(t,false,bool);
warptoif(t,A);
print("Aにワープしませんでした"); //これは呼ばれる;
point A;
=(t,true);
warptoif(t,B);
print("Bにワープしませんでした"); //これは呼ばれない;
point B;
```
warptoif(条件(bool),ラベル)で条件付きでワープするようにできます.

#### warpto,warptoifを使ったループ

```wapl
=(i,0,i32);
point LoopStart;
warptoif(>=(i,5),Break);
+=(i,1);
print(i);
warpto(LoopStart);
point Break;
```
このようにすることでループ処理を実装することができます.<br>

#### イテレータを使ったループ

```wapl
iter(vec(1,2,3,4,5),filter(x,==(%(x,2),1)),map(x,print(x))); //1,3,5;
```
iter(配列,処理1,処理2,....)というように書くことができ,配列に対して順に処理をすることができる.<br>
map(x,処理)やfilter(x,条件)のように**x**は直前の処理をされた後の配列の要素を順に代入されその処理が行われる.

#### ifによる条件分岐

```wapl
=(t,true,bool);
=(a,if(t,1,0),i32);
print(a); //1;
```
このようにif(条件,真のとき,偽のとき)というように条件分岐をすることができる.<br>

#### 関数を作らずにifのなかで処理がしたいときはdoを使おう

```wapl
if(false,do(=(a,1,i32),print(a)),do(=(a,2,i32),print(a))); //2;
```
do(処理1,処理2,....)で処理を順にできる.<br>
注意:doの中では新たなスコープにいるため他のスコープの変数は使えず,またその中で定義した変数を外で使うことはできません.
