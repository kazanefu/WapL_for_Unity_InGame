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
println(a);
```
これを実行すると**2**が出力されます.<br>

#### 同じ名前で変数を作ったときの挙動

```wapl
=(a,1,i32);
println(ptr(a)); 
println(type(a));
println(a);
=(a,"hoge",String);
println(ptr(a));
println(type(a));
println(a);
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
println(ptr(a)) は**a**のポインタなので**0**とは限りませんが一度目も二度目も同じ値が出力されます.つまり,同じアドレスに代入され,型や値はあとに代入されたものになります.<br>
#### 型の種類

整数型: i32, i64<br>
浮動小数点数型: f32, f64<br>
文字列型: String<br>
真偽値型: bool<br>
配列: vec<br>
ポインタ型: ptr<br>
イテレータ型: iter<br>
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
//値をvec()でvec化;
=(v,vec(1,2,3,"hoge",true),vec);

//0からで2番目の要素;
=(a,get_at(v,2),i32); //3;

//vecの要素数;
=(length,len(v),i32); //5;

//capacityを指定して定義;
=(v2,vec(),vec_2);

//要素の追加(pushは要素を追加してその);
=(v2,push(v2,1));
=(v2,push(v2,2));

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

//rangei(a,b,c)はi32で[a,b)でaからcずつ足したものの配列(状態2),a,cは省略可能;
//rangefだとf64になる;
=(v7,rangei(0,10,1),vec);
=(v8,rangef(0.0,10.0,0.1),vec);

```
配列は2種類の状態を持ちます.<br>
1. 値がメモリ上の連続して保存されていて,変数として0個目の要素のポインタ,配列の長さ,配列のキャパシティが保持されている状態<br>
2. vec()や状態1をexpand()で展開することで出せるメモリ上に保持されない値の配列の状態<br>

状態2を変数に代入するとメモリを新たに確保して状態1を作ることができます.<br>
状態1を変数に代入すると0個目の要素のポインタ,配列の長さ,配列のキャパシティがコピーされ,要素を取り出そうとしたときに指すメモリはもとのと同じ場所を指します.<br>
注意:push()はlenを変更した後の状態1を返すのでlenを更新するために変数に再代入する必要があります.<br>

### 第2章:関数,制御フロー

#### 関数

```wapl
main();

fn main(){;
    println("Hello main");
    another_function();
};

fn another_function(){;
    println("AnotherFunction");
};

```
**fn**キーワードで始まり,丸かっこ内に引数,波かっこ内に処理を書きます.<br>
波かっこのあとのセミコロンを忘れないように注意してください.

#### 引数,戻り値を持つ関数

```wapl
println(add(1,2));

fn add(i32 x,i32 y){;
    return +(x,y);
};

```
","区切りで 型 変数名の順で半角スペースを空けて書きます.

#### スコープ

```wapl
fn addOne(i32 b){;
    +=(b,1);
    println(a); //2;
    println(b); //3;
};

=(a,2,i32);
addOne(a);
println(b); //b bという変数は存在しないので文字列として"b"と解釈されて表示される;
```
**a**はどの関数の中のスコープにも入ってないところで定義されているためどこからでもアクセスできます.<br>
一方,**b**はaddOneのスコープにあるので他のところからアクセスすることはできません.<br>
```wapl
fn addOne(i32 b){;
    +=(b,1);
    =(c,b,gbl_i32); //gbl_属性をつけて定義;
    println(a); //2;
    println(b); //3;
};

=(a,2,i32);
addOne(a);
println(c);//3;
```
関数のなかで定義された変数でも,型の初めに**gbl_**とgbl属性をつけることでどこからでもアクセスできるようになります.<br>

#### 関数に参照渡し

```wapl
fn addOne(i32 &_b){;
    +=(&_b,1);
};

=(a,2,i32);
addOne(ptr(a));
println(a); //3;
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
println(1);
warpto(A);
println(2); //これは呼ばれない;
point A;
println(3);
```
**point ラベル名**でラベルをつけて,**warpto**でそのラベルに飛びます.

#### 条件分岐のあるワープ
```wapl
=(t,false,bool);
warptoif(t,A);
println("Aにワープしませんでした"); //これは呼ばれる;
point A;
=(t,true);
warptoif(t,B);
println("Bにワープしませんでした"); //これは呼ばれない;
point B;
```
warptoif(条件(bool),ラベル)で条件付きでワープするようにできます.

#### warpto,warptoifを使ったループ

```wapl
=(i,0,i32);
point LoopStart;
warptoif(>=(i,5),Break);
+=(i,1);
println(i);
warpto(LoopStart);
point Break;
```
このようにすることでループ処理を実装することができます.<br>

#### イテレータを使ったループ

```wapl
iter(vec(1,2,3,4,5),filter(x,==(%(x,2),1)),map(x,println(x))); //1,3,5;
```
iter(配列,処理1,処理2,....)というように書くことができ,配列に対して順に処理をすることができる.<br>
map(x,処理)やfilter(x,条件)のように**x**は直前の処理をされた後の配列の要素を順に代入されその処理が行われます.(xの名前は自由です)<br>
ここでの配列は状態2の配列です.

#### ifによる条件分岐

```wapl
=(t,true,bool);
=(a,if(t,1,0),i32);
println(a); //1;
```
このようにif(条件,真のとき,偽のとき)というように条件分岐をすることができます.<br>

#### 関数を作らずにifやmapなどのなかで複数の処理がしたいときはdoを使おう

```wapl
if(false,do(=(a,1,i32),println(a)),do(=(a,2,i32),println(a))); //2;
```
do(処理1,処理2,....)で処理を順にできる.<br>
注意:基本的にdoの中では新たなスコープにいるため他のスコープの変数は使えず,またその中で定義した変数を外で使うことはできません.外の変数を使いたいときはdoの中にtakein(変数1,変数2,...)という処理を書くことで一つ外のスコープの変数を取り込むことができますが,do内でreturnをするとき,takeinで取り込んだ変数も解放してしまうのでそれを避けるにはexpel(変数)でその変数をdoのスコープから追い出し,return時に解放されないようにすることができます.
```wapl
main();
fn main(){;
    =(a,1,i32);
    =(b,2,i32);
    do(takein(a,b),println(a,b),expel(a),return 0); //1,2;
    println(a,b);//1,b;
};
```

### 第3章:疑似メモリとポインタ

#### 内部的な疑似メモリの仕組み

疑似メモリでは10000の要素数の配列をメモリのように扱い,配列型以外のすべての変数は一つの枠だけを占有していて(つまり,一つのアドレスにある値は1bitではない),EnptyAreaで空いてるメモリの始まりと連続している長さが管理されており,alloc(size)(C#のインタプリタ内の関数)で添え字の小さい方からsizeを満たすEmptyAreaを探索し,要件を満たすものがあったらそこからsizeの分だけ疑似メモリを確保して始まりの疑似メモリ配列の添え字をポインタとして返す.また,free(ptr,size)(C#のインタプリタ内の関数)で解放するとEmptyAreaに解放されていた分が返還され,その後connectEnptyMem()(C#のインタプリタ内の関数)によってEmptyAreaで連続しているとこがあれば結合しています.

#### サイズを指定してメモリ確保

```wapl
=(size,10,i32);

=(p,malloc(size),ptr);
```
malloc()はsizeだけ連続したメモリを確保し,先頭のポインタを返します<br>

#### メモリ領域を特定の値で埋める

```wapl
=(size,10,i32);
=(p,malloc(size),ptr);

memset(p,"hoge",size);
```

memset()は先頭のポインタ,値,サイズを引数に渡して先頭からサイズ分の領域を値で埋めます<br>

#### メモリ領域をコピー

```wapl
=(size,10,i32);
=(src,malloc(size),ptr);
=(dest,malloc(size),ptr);
memset(src,"hoge",size);

memcpy(dest,src,size);
```
memcpy()はメモリ領域srcの先頭からsize分をメモリ領域destにコピーします<br>

### 第4章 イテレータ関連の関数

#### イテレータの仕組み
イテレータ型の構造は配列(状態2),配列のサイズ,配列のどこにいるかの要素を持ち,next()で次に進めて,peek()でいる場所の値を返します.<br>
```wapl
=(i,vec(4,5,2,3),iter);
println(peek(i));//4;
next(i);
println(peek(i));//5;
next(i);
println(peek(i));//2;
next(i);
println(peek(i));//3;
begin(i);
println(peek(i));//4;
end(i);
println(peek(i));//3;
```

iter(配列(状態2),処理1,処理2,....,処理n)と書いたとき,処理kの結果(配列型(状態２))に処理k+1を作用させるということを繰り返して最終的な処理nの結果を返します.配列は状態2で扱うので,最初に与える配列には影響はもたらしません.

#### イテレータの仕組みを使った関数

**filter**
```wapl
=(v,iter(rangei(10),filter(x,==(%(x,2),as(type(x),0)))),vec); //0,2,4,6,8;
```
filter(引数,条件)は条件を満たすときのみその要素を処理結果に加える関数です

**rev**
```wapl
=(v,iter(rangei(5),rev()),vec); //4,3,2,1,0;
```
rev()は配列を反転させます

**map**
```wapl
=(v,iter(rangei(3),map(x,+(x,1))),vec);//0,1,2 -> 1,2,3;
```
map(引数,返り値)は要素を変換できます.<br>

### 第5章 UnityのVector3,GameObject,キー入力

#### Vector3

```wapl
=(v3,vec3(4.0,3.0,5.0),vec3);

println(v3(x));//4.0;
println(v3(y));//3.0;
println(v3(z));//5.0;
```
Vector3はvec3という名前の型で扱い,キャパシティが3のvecは**vec_3**であるが,**vec3**はアンダースコアをつけません.<br>
また,Unity同様にx,y,zでそれぞれの要素にアクセスが可能です.<br>

#### GameObject

```wapl
//GameObjectを生成(上から順に球体でRigidbodyなし,立方体でRigidbodyなし,球体でRigidbodyあり,立方体でRigidbodyあり);
=(Sphere,gen_obj("sphere"),gob);
=(Cube,gen_obj("cube"),gob);
=(SphereRigid,gen_obj("sphere_rigid"),gob);
=(CubeRigid,gen_obj("cube_rigid"),gob);

//名前でGameObjectを探して一つ取得;
=(object,find_object("CubePrefab(Clone)"),gob);
//TagでGameObjectを探してすべて取得(配列(状態2));
=(objects,find_object_tag("Created"),vec);//CreatedはWapLで生成したGameObjectにつくタグ;

//座標の取得,変更;
=(pos,get_position(object),vec3);
set_position(object,vec3(0,0,0));

//向きの取得,変更;
=(rot,get_rotation(object),vec3);
set_rotation(object,vec3(0,0,0));

//大きさの取得,変更;
=(sca,get_scale(object),vec3);
set_scale(object,vec3(1,1,1));

//名前の取得,変更;
=(name,get_name(object),String);
set_name(object,"HOGE");

//transform.forward,up,rightの取得
=(forward,get_forward(object),vec3);
=(up,get_up(object),vec3);
=(right,get_right(object),vec3);

//activeSelfの取得,変更;
=(isActive,get_active(object),bool);
set_active(object,false);
set_active(object,true);

//GameObjectの削除;
destroy(object,1);//1秒後に削除,時間は省略可;
free(object);//ちゃんとメモリ解放しましょう;

//Rigidbodyの扱い;
addforce(CubeRigid,vec3(1,1,1));//Addforce;
=(velocity,get_velocity(CubeRigid),vec3);//velocityの取得;
set_velocity(CubeRigid,vec3(0,0,0));//velocityの変更;
set_gravity(CubeRigid,false);//重力が有効かを設定する;
```
**set_**で始まるGameObject関連の関数はすべて1つ目の引数で渡しているGameObject自身を返すので<br>
```wapl
set_active(set_position(set_name(gen_obj("cube"),CUBE),vec3(0,0,0)),false);
```
のようにまとめて設定することもできます.


#### キー入力
```wapl
=(A,keyboard_A,bool);
```
keyboard_"KeyCode"でそのキーが今押されている状態化を取得できる。

### 第6章 毎フレーム実行する


