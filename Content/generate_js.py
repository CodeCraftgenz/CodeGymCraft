import json, os

OUT = os.path.join(os.path.dirname(__file__), "challenges")
os.makedirs(OUT, exist_ok=True)

def save(data):
    with open(os.path.join(OUT, f"{data['id']}.json"), "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

def test(tests_code):
    return f"function __runTests() {{\n    const results = [];\n{tests_code}\n    return results;\n}}"

def t(expr, msg_ok, msg_fail):
    return f"    try {{ const r = {expr}; results.push({{ pass: !!r, message: r ? '{msg_ok}' : '{msg_fail}' }}); }} catch(e) {{ results.push({{ pass: false, message: 'Erro: ' + e.message }}); }}"

def teq(expr, expected, msg_ok, msg_fail):
    exp_s = json.dumps(expected) if isinstance(expected, (list,dict,str)) else str(expected)
    return f"    try {{ const r = {expr}; const exp = {exp_s}; const pass = JSON.stringify(r) === JSON.stringify(exp); results.push({{ pass, message: pass ? '{msg_ok}' : '{msg_fail}: esperado ' + JSON.stringify(exp) + ' obteve ' + JSON.stringify(r) }}); }} catch(e) {{ results.push({{ pass: false, message: 'Erro: ' + e.message }}); }}"

js_ini = [
    ("Variáveis Let e Const","Declare variáveis corretamente.\n\nCrie uma função `criarVariaveis()` que retorne um objeto com nome (string) e idade (number).",["variáveis","tipos"],
     "// Crie a função criarVariaveis\nfunction criarVariaveis() {\n    // Retorne { nome: 'seu nome', idade: sua idade }\n}\n",
     test(teq("criarVariaveis().nome !== undefined && typeof criarVariaveis().nome === 'string'","true","Nome é string!","Nome deve ser string") + "\n" +
          teq("typeof criarVariaveis().idade","'number'","Idade é number!","Idade deve ser number"))),
    ("Soma de Números","Crie função `soma(a, b)` que retorna a soma.",["operadores","função"],
     "function soma(a, b) {\n    // Retorne a soma de a e b\n}\n",
     test(teq("soma(2, 3)","5","soma(2,3) = 5!","soma(2,3) incorreto") + "\n" +
          teq("soma(-1, 1)","0","soma(-1,1) = 0!","soma(-1,1) incorreto") + "\n" +
          teq("soma(0, 0)","0","soma(0,0) = 0!","soma(0,0) incorreto"))),
    ("Par ou Ímpar","Crie `parOuImpar(n)` que retorna 'par' ou 'ímpar'.",["if/else","condição"],
     "function parOuImpar(n) {\n    // Retorne 'par' ou 'ímpar'\n}\n",
     test(teq("parOuImpar(4)","'par'","4 é par!","4 deveria ser par") + "\n" +
          teq("parOuImpar(7)","'ímpar'","7 é ímpar!","7 deveria ser ímpar") + "\n" +
          teq("parOuImpar(0)","'par'","0 é par!","0 deveria ser par"))),
    ("Maior de Dois","Crie `maior(a, b)` que retorna o maior.",["comparação","if"],
     "function maior(a, b) {\n    // Retorne o maior valor\n}\n",
     test(teq("maior(5, 3)","5","maior(5,3) ok!","Incorreto") + "\n" +
          teq("maior(1, 9)","9","maior(1,9) ok!","Incorreto") + "\n" +
          teq("maior(4, 4)","4","maior(4,4) ok!","Incorreto"))),
    ("Classificar Nota","Crie `classificar(nota)`: A(>=90), B(>=80), C(>=70), D(>=60), F(<60).",["if/else","lógica"],
     "function classificar(nota) {\n    // Retorne 'A', 'B', 'C', 'D' ou 'F'\n}\n",
     test(teq("classificar(95)","'A'","95=A!","Incorreto") + "\n" +
          teq("classificar(85)","'B'","85=B!","Incorreto") + "\n" +
          teq("classificar(55)","'F'","55=F!","Incorreto"))),
    ("Valor Absoluto","Crie `absoluto(n)` que retorna o valor absoluto.",["ternário","math"],
     "function absoluto(n) {\n    // Retorne o valor absoluto sem usar Math.abs\n}\n",
     test(teq("absoluto(-5)","5","-5→5 ok!","Incorreto") + "\n" +
          teq("absoluto(3)","3","3→3 ok!","Incorreto") + "\n" +
          teq("absoluto(0)","0","0→0 ok!","Incorreto"))),
    ("Dia da Semana","Crie `diaDaSemana(n)` (1=Domingo...7=Sábado).",["switch","lógica"],
     "function diaDaSemana(n) {\n    // 1=Domingo, 2=Segunda, ..., 7=Sábado\n}\n",
     test(teq("diaDaSemana(1)","'Domingo'","1=Domingo!","Incorreto") + "\n" +
          teq("diaDaSemana(2)","'Segunda'","2=Segunda!","Incorreto") + "\n" +
          teq("diaDaSemana(7)","'Sábado'","7=Sábado!","Incorreto"))),
    ("Contar até N","Crie `contarAte(n)` que retorna array [1..n].",["loop","array"],
     "function contarAte(n) {\n    // Retorne array [1, 2, ..., n]\n}\n",
     test(teq("contarAte(5)","[1,2,3,4,5]","contarAte(5) ok!","Incorreto") + "\n" +
          teq("contarAte(1)","[1]","contarAte(1) ok!","Incorreto"))),
    ("Somar até N","Crie `somarAte(n)` soma de 1 a n.",["loop","soma"],
     "function somarAte(n) {\n    // Retorne 1 + 2 + ... + n\n}\n",
     test(teq("somarAte(5)","15","somarAte(5)=15!","Incorreto") + "\n" +
          teq("somarAte(10)","55","somarAte(10)=55!","Incorreto"))),
    ("Somar Array","Crie `somarArray(arr)` soma de todos os elementos.",["array","loop"],
     "function somarArray(arr) {\n    // Retorne a soma de todos os elementos\n}\n",
     test(teq("somarArray([1,2,3])","6","[1,2,3]=6!","Incorreto") + "\n" +
          teq("somarArray([10,-5,5])","10","[10,-5,5]=10!","Incorreto") + "\n" +
          teq("somarArray([])","0","[]=0!","Incorreto"))),
    ("Dobro","Crie `dobro(n)` que retorna n*2.",["função","retorno"],
     "function dobro(n) {\n    // Retorne n * 2\n}\n",
     test(teq("dobro(5)","10","dobro(5)=10!","Incorreto") + "\n" +
          teq("dobro(0)","0","dobro(0)=0!","Incorreto"))),
    ("Área do Retângulo","Crie `areaRetangulo(b, h)` base * altura.",["função","cálculo"],
     "function areaRetangulo(b, h) {\n    // Retorne base * altura\n}\n",
     test(teq("areaRetangulo(5, 3)","15","5×3=15!","Incorreto") + "\n" +
          teq("areaRetangulo(10, 10)","100","10×10=100!","Incorreto"))),
    ("Arrow Function","Crie `quadrado` como arrow function.",["arrow","função"],
     "// Crie como arrow function\nconst quadrado = (n) => {\n    // Retorne n ao quadrado\n};\n",
     test(teq("quadrado(4)","16","4²=16!","Incorreto") + "\n" +
          teq("quadrado(0)","0","0²=0!","Incorreto"))),
    ("Primeira Letra","Crie `primeiraLetra(str)` retorna primeiro caractere.",["string","charAt"],
     "function primeiraLetra(str) {\n    // Retorne o primeiro caractere\n}\n",
     test(teq("primeiraLetra('Hello')","'H'","H ok!","Incorreto") + "\n" +
          teq("primeiraLetra('abc')","'a'","a ok!","Incorreto"))),
    ("Gritar","Crie `grito(str)` retorna em MAIÚSCULAS.",["string","toUpperCase"],
     "function grito(str) {\n    // Retorne em maiúsculas\n}\n",
     test(teq("grito('hello')","'HELLO'","HELLO ok!","Incorreto") + "\n" +
          teq("grito('abc')","'ABC'","ABC ok!","Incorreto"))),
    ("Contém Palavra","Crie `contemPalavra(str, palavra)` retorna true/false.",["string","includes"],
     "function contemPalavra(str, palavra) {\n    // Retorne true se str contém palavra\n}\n",
     test(teq("contemPalavra('Hello World', 'World')","true","World encontrado!","Incorreto") + "\n" +
          teq("contemPalavra('Hello', 'xyz')","false","xyz não encontrado!","Incorreto"))),
    ("Primeiros N Caracteres","Crie `primeiros(str, n)` retorna os primeiros n chars.",["string","slice"],
     "function primeiros(str, n) {\n    // Retorne os primeiros n caracteres\n}\n",
     test(teq("primeiros('JavaScript', 4)","'Java'","Java ok!","Incorreto") + "\n" +
          teq("primeiros('abc', 2)","'ab'","ab ok!","Incorreto"))),
    ("Inverter Palavras","Crie `inverterPalavras(str)` inverte ordem das palavras.",["string","split/join"],
     "function inverterPalavras(str) {\n    // 'hello world' -> 'world hello'\n}\n",
     test(teq("inverterPalavras('hello world')","'world hello'","Invertido!","Incorreto") + "\n" +
          teq("inverterPalavras('a b c')","'c b a'","abc→cba ok!","Incorreto"))),
    ("Saudação","Crie `saudacao(nome)` com template literal.",["string","template literal"],
     "function saudacao(nome) {\n    // Retorne 'Olá, {nome}!' usando template literal\n}\n",
     test(teq("saudacao('Maria')","'Olá, Maria!'","Maria ok!","Incorreto") + "\n" +
          teq("saudacao('João')","'Olá, João!'","João ok!","Incorreto"))),
    ("Último Elemento","Crie `ultimoElemento(arr)` retorna o último.",["array","index"],
     "function ultimoElemento(arr) {\n    // Retorne o último elemento\n}\n",
     test(teq("ultimoElemento([1,2,3])","3","3 ok!","Incorreto") + "\n" +
          teq("ultimoElemento(['a'])","'a'","a ok!","Incorreto"))),
    ("Encontrar no Array","Crie `encontrar(arr, item)` retorna index ou -1.",["array","indexOf"],
     "function encontrar(arr, item) {\n    // Retorne o índice ou -1\n}\n",
     test(teq("encontrar([1,2,3], 2)","1","Index 1 ok!","Incorreto") + "\n" +
          teq("encontrar([1,2,3], 5)","-1","-1 ok!","Incorreto"))),
    ("Criar Pessoa","Crie `criarPessoa(nome, idade)` retorna objeto.",["objeto","criação"],
     "function criarPessoa(nome, idade) {\n    // Retorne { nome, idade }\n}\n",
     test(teq("criarPessoa('Ana', 25).nome","'Ana'","Nome ok!","Incorreto") + "\n" +
          teq("criarPessoa('Ana', 25).idade","25","Idade ok!","Incorreto"))),
    ("Acessar Propriedade","Crie `getNome(pessoa)` retorna nome.",["objeto","acesso"],
     "function getNome(pessoa) {\n    // Retorne pessoa.nome\n}\n",
     test(teq("getNome({nome:'Bob',idade:30})","'Bob'","Bob ok!","Incorreto"))),
    ("Arredondar","Crie `arredondar(n)` arredonda para baixo.",["Math","floor"],
     "function arredondar(n) {\n    // Use Math.floor\n}\n",
     test(teq("arredondar(4.7)","4","4.7→4!","Incorreto") + "\n" +
          teq("arredondar(1.1)","1","1.1→1!","Incorreto"))),
    ("Aleatório Entre","Crie `aleatorioEntre(min, max)` retorna inteiro entre min e max.",["Math","random"],
     "function aleatorioEntre(min, max) {\n    // Retorne inteiro aleatório entre min e max (inclusive)\n}\n",
     test(t("(function(){ const r = aleatorioEntre(1,10); return r >= 1 && r <= 10; })()","Valor no range!","Fora do range") + "\n" +
          t("Number.isInteger(aleatorioEntre(1,10))","É inteiro!","Deve ser inteiro"))),
    ("Somar Strings","Crie `somarStrings(a, b)` soma strings numéricas.",["parsing","Number"],
     "function somarStrings(a, b) {\n    // '5' + '3' = 8 (number, não string)\n}\n",
     test(teq("somarStrings('5', '3')","8","5+3=8!","Incorreto") + "\n" +
          teq("typeof somarStrings('1','2')","'number'","É number!","Deve ser number"))),
    ("É Truthy","Crie `isTruthy(val)` retorna true/false.",["boolean","truthy"],
     "function isTruthy(val) {\n    // Retorne true se val é truthy\n}\n",
     test(teq("isTruthy(1)","true","1 é truthy!","Incorreto") + "\n" +
          teq("isTruthy(0)","false","0 é falsy!","Incorreto") + "\n" +
          teq("isTruthy('')","false","'' é falsy!","Incorreto"))),
    ("Tipo Igual","Crie `tipoIgual(a, b)` verifica se mesmo tipo.",["typeof","comparação"],
     "function tipoIgual(a, b) {\n    // Retorne true se a e b são do mesmo tipo\n}\n",
     test(teq("tipoIgual(1, 2)","true","Ambos number!","Incorreto") + "\n" +
          teq("tipoIgual(1, '1')","false","Tipos diferentes!","Incorreto"))),
    ("FizzBuzz","Crie `fizzBuzz(n)`: múltiplo de 3='Fizz', 5='Buzz', ambos='FizzBuzz'.",["lógica","condição"],
     "function fizzBuzz(n) {\n    // Retorne 'Fizz', 'Buzz', 'FizzBuzz' ou n\n}\n",
     test(teq("fizzBuzz(15)","'FizzBuzz'","15=FizzBuzz!","Incorreto") + "\n" +
          teq("fizzBuzz(9)","'Fizz'","9=Fizz!","Incorreto") + "\n" +
          teq("fizzBuzz(10)","'Buzz'","10=Buzz!","Incorreto") + "\n" +
          teq("fizzBuzz(7)","7","7=7!","Incorreto"))),
    ("Reverter String","Crie `reverter(str)` inverte a string.",["string","reverse"],
     "function reverter(str) {\n    // 'abc' -> 'cba'\n}\n",
     test(teq("reverter('hello')","'olleh'","olleh ok!","Incorreto") + "\n" +
          teq("reverter('abc')","'cba'","cba ok!","Incorreto"))),
]

for i,(t,d,tg,s,tc) in enumerate(js_ini,1):
    save({"id":f"js-ini-{i:03d}","track":"javascript","title":t,"description":d,"starterCode":s,
          "tags":tg,"difficulty":"Iniciante","validatorType":"js-tests","validatorConfig":{"testCode":tc}})

js_int = [
    ("Array Map","Crie `dobrarTodos(arr)` usando map.",["array","map"],
     "function dobrarTodos(arr) {\n    // Use map para dobrar todos\n}\n",
     test(teq("dobrarTodos([1,2,3])","[2,4,6]","[2,4,6] ok!","Incorreto"))),
    ("Array Filter","Crie `apenasPositivos(arr)` usando filter.",["array","filter"],
     "function apenasPositivos(arr) {\n    // Retorne apenas positivos\n}\n",
     test(teq("apenasPositivos([1,-2,3,-4,5])","[1,3,5]","Filtrado!","Incorreto"))),
    ("Array Reduce","Crie `somarTodos(arr)` usando reduce.",["array","reduce"],
     "function somarTodos(arr) {\n    // Use reduce para somar\n}\n",
     test(teq("somarTodos([1,2,3,4])","10","10 ok!","Incorreto") + "\n" + teq("somarTodos([])","0","[] ok!","Incorreto"))),
    ("Array Find","Crie `encontrarPar(arr)` retorna primeiro par.",["array","find"],
     "function encontrarPar(arr) {\n    // Retorne o primeiro número par\n}\n",
     test(teq("encontrarPar([1,3,4,6])","4","4 ok!","Incorreto"))),
    ("Array Some/Every","Crie `todosPares(arr)` e `algumPar(arr)`.",["array","some/every"],
     "function todosPares(arr) {\n    // true se todos são pares\n}\nfunction algumPar(arr) {\n    // true se algum é par\n}\n",
     test(teq("todosPares([2,4,6])","true","Todos pares!","Incorreto") + "\n" + teq("algumPar([1,3,4])","true","Algum par!","Incorreto"))),
    ("Array Sort","Crie `ordenar(arr)` ordena números crescente.",["array","sort"],
     "function ordenar(arr) {\n    // Ordene numericamente (crescente)\n}\n",
     test(teq("ordenar([3,1,4,1,5])","[1,1,3,4,5]","Ordenado!","Incorreto"))),
    ("Array Flat","Crie `achatar(arr)` que achata 1 nível.",["array","flat"],
     "function achatar(arr) {\n    // Achate um nível\n}\n",
     test(teq("achatar([[1,2],[3,4]])","[1,2,3,4]","Achatado!","Incorreto"))),
    ("Destructuring Array","Crie `primeiroEUltimo(arr)` retorna [primeiro, último].",["destructuring","array"],
     "function primeiroEUltimo(arr) {\n    // Use destructuring\n}\n",
     test(teq("primeiroEUltimo([1,2,3,4,5])","[1,5]","[1,5] ok!","Incorreto"))),
    ("Destructuring Objeto","Crie `extrair(obj)` retorna nome e idade.",["destructuring","objeto"],
     "function extrair(obj) {\n    // Use destructuring para retornar { nome, idade }\n}\n",
     test(teq("extrair({nome:'Ana',idade:25,cidade:'SP'}).nome","'Ana'","Nome ok!","Incorreto"))),
    ("Spread Array","Crie `juntar(a, b)` junta dois arrays.",["spread","array"],
     "function juntar(a, b) {\n    // Use spread operator\n}\n",
     test(teq("juntar([1,2],[3,4])","[1,2,3,4]","Juntado!","Incorreto"))),
    ("Spread Objeto","Crie `mesclar(a, b)` mescla objetos.",["spread","objeto"],
     "function mesclar(a, b) {\n    // Use spread para mesclar\n}\n",
     test(teq("mesclar({x:1},{y:2}).y","2","y=2 ok!","Incorreto"))),
    ("Rest Parameters","Crie `somarVarios(...nums)` soma todos.",["rest","parâmetros"],
     "function somarVarios(...nums) {\n    // Some todos os números\n}\n",
     test(teq("somarVarios(1,2,3)","6","6 ok!","Incorreto") + "\n" + teq("somarVarios(10)","10","10 ok!","Incorreto"))),
    ("Default Parameters","Crie `saudar(nome='Visitante')` com default.",["default","parâmetros"],
     "function saudar(nome = 'Visitante') {\n    // Retorne 'Olá, {nome}!'\n}\n",
     test(teq("saudar()","'Olá, Visitante!'","Default ok!","Incorreto") + "\n" + teq("saudar('Ana')","'Olá, Ana!'","Ana ok!","Incorreto"))),
    ("Closure","Crie `contador()` que retorna função incrementadora.",["closure","função"],
     "function contador() {\n    // Retorne uma função que incrementa e retorna o valor\n}\n",
     test("    try { const c = contador(); const r1 = c(); const r2 = c(); results.push({ pass: r1===1&&r2===2, message: r1===1&&r2===2 ? 'Contador incrementa!' : 'Esperado 1,2 obteve '+r1+','+r2 }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Callback","Crie `executar(arr, callback)` aplica callback a cada item.",["callback","função"],
     "function executar(arr, callback) {\n    // Retorne novo array aplicando callback\n}\n",
     test(teq("executar([1,2,3], x => x*2)","[2,4,6]","Callback ok!","Incorreto"))),
    ("Promise Básico","Crie `esperar(ms)` retorna Promise.",["promise","async"],
     "function esperar(ms) {\n    // Retorne Promise que resolve após ms\n    return new Promise((resolve) => {\n        // TODO\n    });\n}\n",
     test("    try { const p = esperar(10); results.push({ pass: p instanceof Promise, message: p instanceof Promise ? 'É Promise!' : 'Não é Promise' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Promise All","Crie `executarTodas(promises)` com Promise.all.",["promise","all"],
     "function executarTodas(promises) {\n    // Use Promise.all\n}\n",
     test("    try { const r = executarTodas([Promise.resolve(1), Promise.resolve(2)]); results.push({ pass: r instanceof Promise, message: r instanceof Promise ? 'Retorna Promise!' : 'Deve retornar Promise' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Async/Await","Crie função async `buscarDado()`.",["async","await"],
     "async function buscarDado() {\n    // Retorne 'dado' após await\n    return 'dado';\n}\n",
     test("    try { const r = buscarDado(); results.push({ pass: r instanceof Promise, message: r instanceof Promise ? 'É async!' : 'Deve ser async' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Try/Catch","Crie `seguro(fn)` que captura erros.",["error","try/catch"],
     "function seguro(fn) {\n    // Execute fn, retorne resultado ou 'erro'\n}\n",
     test(teq("seguro(() => 42)","42","42 ok!","Incorreto") + "\n" + teq("seguro(() => { throw new Error('x') })","'erro'","Erro capturado!","Incorreto"))),
    ("Classe ES6","Crie classe `Animal` com nome e falar().",["classe","OOP"],
     "class Animal {\n    // Construtor com nome\n    // Método falar() retorna 'O {nome} faz som'\n}\n",
     test("    try { const a = new Animal('Rex'); results.push({ pass: a.falar() === 'O Rex faz som', message: a.falar() === 'O Rex faz som' ? 'Falar ok!' : 'Incorreto: ' + a.falar() }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Herança","Crie `Cachorro extends Animal` com latir().",["herança","extends"],
     "class Animal {\n    constructor(nome) { this.nome = nome; }\n    falar() { return `O ${this.nome} faz som`; }\n}\n\nclass Cachorro extends Animal {\n    // latir() retorna '{nome} diz: Au au!'\n}\n",
     test("    try { const d = new Cachorro('Rex'); results.push({ pass: d.latir() === 'Rex diz: Au au!', message: d.latir() === 'Rex diz: Au au!' ? 'Latir ok!' : 'Incorreto: ' + d.latir() }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }" + "\n" +
          "    try { const d = new Cachorro('Rex'); results.push({ pass: d.falar() === 'O Rex faz som', message: d.falar() === 'O Rex faz som' ? 'Herança ok!' : 'Falar herdado incorreto' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Getter/Setter","Crie classe com get/set.",["classe","getter/setter"],
     "class Temperatura {\n    constructor(celsius) { this._celsius = celsius; }\n    // get fahrenheit\n    // set fahrenheit\n}\n",
     test("    try { const t = new Temperatura(0); results.push({ pass: t.fahrenheit === 32, message: t.fahrenheit === 32 ? '0°C = 32°F!' : 'Incorreto: ' + t.fahrenheit }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Static Method","Crie método estático.",["classe","static"],
     "class MathUtil {\n    // static soma(a, b)\n}\n",
     test(teq("MathUtil.soma(2, 3)","5","Estático ok!","Incorreto"))),
    ("Regex Test","Crie `isEmail(str)` com regex.",["regex","validação"],
     "function isEmail(str) {\n    // Retorne true se parece email válido\n}\n",
     test(teq("isEmail('a@b.com')","true","Email válido!","Incorreto") + "\n" + teq("isEmail('abc')","false","Não é email!","Incorreto"))),
    ("Regex Replace","Crie `censurar(str)` substitui palavrões por ***.",["regex","replace"],
     "function censurar(str, palavra) {\n    // Substitua todas ocorrências de palavra por '***'\n}\n",
     test(teq("censurar('foo bar foo', 'foo')","'*** bar ***'","Censurado!","Incorreto"))),
    ("Set","Crie `unicos(arr)` retorna array sem duplicatas.",["Set","único"],
     "function unicos(arr) {\n    // Use Set para remover duplicatas\n}\n",
     test(teq("unicos([1,2,2,3,3])","[1,2,3]","Únicos!","Incorreto"))),
    ("Map","Crie `contarLetras(str)` retorna Map de contagens.",["Map","contagem"],
     "function contarLetras(str) {\n    // Retorne Map com contagem de cada letra\n}\n",
     test("    try { const m = contarLetras('aab'); results.push({ pass: m.get('a')===2, message: m.get('a')===2 ? 'a=2 ok!' : 'Incorreto' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Optional Chaining","Crie `getCidade(pessoa)` com ?.",["optional","chaining"],
     "function getCidade(pessoa) {\n    // Use ?. para acessar pessoa.endereco.cidade\n    // Retorne undefined se não existir\n}\n",
     test(teq("getCidade({endereco:{cidade:'SP'}})","'SP'","SP ok!","Incorreto") + "\n" +
          teq("getCidade({})","undefined","Undefined ok!","Incorreto"))),
    ("Nullish Coalescing","Crie `padrao(val, def)` com ??.",["nullish","coalescing"],
     "function padrao(val, def) {\n    // Use ?? para retornar def se val é null/undefined\n}\n",
     test(teq("padrao(null, 'default')","'default'","Default ok!","Incorreto") + "\n" +
          teq("padrao(0, 'default')","0","0 mantido!","Incorreto"))),
]

for i,(t,d,tg,s,tc) in enumerate(js_int,1):
    save({"id":f"js-int-{i:03d}","track":"javascript","title":t,"description":d,"starterCode":s,
          "tags":tg,"difficulty":"Intermediario","validatorType":"js-tests","validatorConfig":{"testCode":tc}})

js_adv = [
    ("Bubble Sort","Implemente bubble sort.",["algoritmo","sort"],
     "function bubbleSort(arr) {\n    // Implemente bubble sort\n}\n",
     test(teq("bubbleSort([3,1,4,1,5])","[1,1,3,4,5]","Ordenado!","Incorreto"))),
    ("Binary Search","Implemente busca binária.",["algoritmo","search"],
     "function binarySearch(arr, target) {\n    // Retorne index ou -1\n}\n",
     test(teq("binarySearch([1,2,3,4,5], 3)","2","Index 2!","Incorreto") + "\n" + teq("binarySearch([1,2,3], 4)","-1","-1 ok!","Incorreto"))),
    ("Fatorial Recursivo","Implemente fatorial com recursão.",["recursão","fatorial"],
     "function fatorial(n) {\n    // Recursão: n! = n * (n-1)!\n}\n",
     test(teq("fatorial(5)","120","5!=120!","Incorreto") + "\n" + teq("fatorial(0)","1","0!=1!","Incorreto"))),
    ("Stack","Implemente uma Stack.",["estrutura","stack"],
     "class Stack {\n    constructor() { this.items = []; }\n    // push(item), pop(), peek(), isEmpty(), size()\n}\n",
     test("    try { const s = new Stack(); s.push(1); s.push(2); const r = s.pop(); results.push({ pass: r===2 && s.peek()===1, message: r===2 ? 'Stack ok!' : 'Incorreto' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Queue","Implemente uma Queue.",["estrutura","queue"],
     "class Queue {\n    constructor() { this.items = []; }\n    // enqueue(item), dequeue(), front(), isEmpty(), size()\n}\n",
     test("    try { const q = new Queue(); q.enqueue(1); q.enqueue(2); const r = q.dequeue(); results.push({ pass: r===1, message: r===1 ? 'Queue ok! FIFO correto' : 'Incorreto' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Linked List","Implemente lista encadeada básica.",["estrutura","linked list"],
     "class Node {\n    constructor(val) { this.val = val; this.next = null; }\n}\nclass LinkedList {\n    constructor() { this.head = null; }\n    // append(val), toArray(), size()\n}\n",
     test("    try { const ll = new LinkedList(); ll.append(1); ll.append(2); ll.append(3); const r = ll.toArray(); results.push({ pass: JSON.stringify(r)==='[1,2,3]', message: JSON.stringify(r)==='[1,2,3]' ? 'Lista ok!' : 'Incorreto: ' + JSON.stringify(r) }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Debounce","Implemente debounce.",["padrão","debounce"],
     "function debounce(fn, delay) {\n    // Retorne função debounced\n}\n",
     test("    try { let count = 0; const d = debounce(() => count++, 100); d(); d(); d(); results.push({ pass: typeof d === 'function', message: typeof d === 'function' ? 'Retorna função!' : 'Deve retornar função' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Deep Clone","Crie `deepClone(obj)` clone profundo.",["clone","objeto"],
     "function deepClone(obj) {\n    // Clone profundo sem referências compartilhadas\n}\n",
     test("    try { const o = {a:{b:1}}; const c = deepClone(o); o.a.b = 2; results.push({ pass: c.a.b===1, message: c.a.b===1 ? 'Deep clone ok!' : 'Referência compartilhada!' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Flatten Recursivo","Crie `flatten(arr)` achata recursivamente.",["recursão","array"],
     "function flatten(arr) {\n    // Achate todos os níveis\n}\n",
     test(teq("flatten([1,[2,[3,[4]]]])","[1,2,3,4]","Achatado!","Incorreto"))),
    ("Curry","Implemente `curry(fn)` currying.",["funcional","curry"],
     "function curry(fn) {\n    // Retorne versão curried de fn\n}\n",
     test("    try { const add = curry((a,b,c) => a+b+c); results.push({ pass: add(1)(2)(3)===6, message: add(1)(2)(3)===6 ? 'Curry ok!' : 'Incorreto' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Memoize","Implemente memoização.",["otimização","memoize"],
     "function memoize(fn) {\n    // Retorne versão memoizada\n}\n",
     test("    try { let calls = 0; const fn = memoize(x => { calls++; return x*2; }); fn(5); fn(5); results.push({ pass: calls===1, message: calls===1 ? 'Memoizado!' : 'Chamou '+calls+' vezes' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Observer Pattern","Implemente EventEmitter.",["padrão","observer"],
     "class EventEmitter {\n    constructor() { this.events = {}; }\n    // on(event, handler), emit(event, ...args), off(event, handler)\n}\n",
     test("    try { const em = new EventEmitter(); let val = 0; em.on('test', x => val = x); em.emit('test', 42); results.push({ pass: val===42, message: val===42 ? 'Observer ok!' : 'Incorreto: ' + val }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Iterator","Crie objeto iterável.",["iterator","protocol"],
     "function range(start, end) {\n    // Retorne objeto iterável de start a end\n    return {\n        [Symbol.iterator]() {\n            // TODO\n        }\n    };\n}\n",
     test("    try { const r = [...range(1,4)]; results.push({ pass: JSON.stringify(r)==='[1,2,3,4]', message: JSON.stringify(r)==='[1,2,3,4]' ? 'Iterável!' : 'Incorreto: ' + JSON.stringify(r) }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Generator","Crie generator de Fibonacci.",["generator","fibonacci"],
     "function* fibonacci() {\n    // yield sequência de Fibonacci\n}\n",
     test("    try { const gen = fibonacci(); const vals = [gen.next().value, gen.next().value, gen.next().value, gen.next().value, gen.next().value]; results.push({ pass: JSON.stringify(vals)==='[0,1,1,2,3]', message: JSON.stringify(vals)==='[0,1,1,2,3]' ? 'Fibonacci ok!' : 'Incorreto: ' + JSON.stringify(vals) }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Proxy","Crie Proxy que valida tipos.",["proxy","metaprogramação"],
     "function criarProxyValidado(obj, schema) {\n    // Proxy que valida tipos ao setar\n    return new Proxy(obj, {\n        set(target, prop, value) {\n            // TODO: valide tipo\n            target[prop] = value;\n            return true;\n        }\n    });\n}\n",
     test("    try { const p = criarProxyValidado({}, {name:'string',age:'number'}); p.name = 'Ana'; results.push({ pass: p.name==='Ana', message: p.name==='Ana' ? 'Proxy ok!' : 'Incorreto' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Promise.allSettled","Use allSettled.",["promise","allSettled"],
     "function executarTodas(promises) {\n    // Use Promise.allSettled\n    return Promise.allSettled(promises);\n}\n",
     test("    try { const r = executarTodas([Promise.resolve(1), Promise.reject('err')]); results.push({ pass: r instanceof Promise, message: r instanceof Promise ? 'Promise ok!' : 'Deve ser Promise' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Throttle","Implemente throttle.",["padrão","throttle"],
     "function throttle(fn, limit) {\n    // Retorne função throttled\n}\n",
     test("    try { const t = throttle(() => {}, 100); results.push({ pass: typeof t === 'function', message: typeof t === 'function' ? 'Retorna função!' : 'Incorreto' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Compose","Implemente compose(f, g, ...).",["funcional","compose"],
     "function compose(...fns) {\n    // compose(f, g)(x) = f(g(x))\n}\n",
     test("    try { const r = compose(x => x+1, x => x*2)(3); results.push({ pass: r===7, message: r===7 ? 'compose ok! (3*2)+1=7' : 'Incorreto: ' + r }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Pipe","Implemente pipe (inverso de compose).",["funcional","pipe"],
     "function pipe(...fns) {\n    // pipe(f, g)(x) = g(f(x))\n}\n",
     test("    try { const r = pipe(x => x*2, x => x+1)(3); results.push({ pass: r===7, message: r===7 ? 'pipe ok! (3*2)+1=7' : 'Incorreto: ' + r }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
    ("Imutabilidade","Crie `atualizarProp(obj, key, val)` sem mutar original.",["imutável","spread"],
     "function atualizarProp(obj, key, val) {\n    // Retorne novo objeto sem mutar original\n}\n",
     test("    try { const o = {a:1,b:2}; const n = atualizarProp(o, 'a', 10); results.push({ pass: n.a===10 && o.a===1, message: n.a===10 && o.a===1 ? 'Imutável!' : 'Original mutado!' }); } catch(e) { results.push({ pass: false, message: 'Erro: ' + e.message }); }")),
]

for i,(t,d,tg,s,tc) in enumerate(js_adv,1):
    save({"id":f"js-adv-{i:03d}","track":"javascript","title":t,"description":d,"starterCode":s,
          "tags":tg,"difficulty":"Avancado","validatorType":"js-tests","validatorConfig":{"testCode":tc}})

print(f"JS: {len(js_ini)} ini + {len(js_int)} int + {len(js_adv)} adv = {len(js_ini)+len(js_int)+len(js_adv)}")
