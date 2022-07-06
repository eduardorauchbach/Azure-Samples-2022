# **CustomLogs**
Classe com toda a abstração de logs customizados. Pensado em execução de escopo, os logs persistem todos os IDs e são gerados com os mesmos somente no dispose do escopo.

## **Inicialização**
 1 - Deve ser adicionado na inicialização da aplicação utilizando o método de extensão AddCustomLog()
No momento o RauchTech.Extensions.Logging já está sendo inicializado nos projetos abaixo:

  - **Functions.Common**
  - **Agents.Common**
  - **API.Common**

Serviços que fazem uso dos projetos acima não necessitam inicializar o RauchTech.Extensions.Logging

```
services.AddCustomLog():
```

2 - Deve ser injetado via construtor na classe que for utilizar, exemplo abaixo: 
```
private readonly ICustomLog _logger;

public SampleClass(ICustomLogFactory customLogFactory)
{
     _logger = customLogFactory.CreateLogger<SampleClass>();
}
```

## **Eventos Padrão de Uso**
<br/>

**AddID**: Usado para cadastrar uma chave de escopo, como o ID de simulação ou analise de crédito. Os IDs serão exibidos mesmo nos Logs gerados antes.
- **string key**: Nome da chave a ser normalizada, em Caml case. Ex de saída: **AsTest => as_test**
- **object value**: Valor qualquer a ser convertido em texto
</br>

**LogCustom**: Usado para cadastrar os Logs no escopo

- **LogLevel logLevel** - Level do Log gerado (Microsoft.Extensions.Logging) (único campo obrigatório)
```
//Controllers ou Triggers, usado na primeira camada ou pontos onde o Log é obrigatório
LogLevel.Information

//Níveis inferiores, no intuito de entender melhor os fluxos internos e auxiliar na depuração
LogLevel.Debug

//Quando objetos ou informações mais específicas são necessárias no fluxo de depuração
LogLevel.Trace

//Quando na parte de validações de fluxo alguma coisa não estiver adequada mas não causar uma Exceção
LogLevel.Warning

//Quando existir uma exceção
LogLevel.Error
```
- **EventId? eventId = null** - Id de Evento pré-cadastrado (Microsoft.Extensions.Logging.EventId)
- **Exception exception = null** - Evento de Exceção
- **string message = null** - Mensagem de texto simples, sem notação de campo de Log ({valor})
- **[CallerMemberName] string memberName = ""** - Não enviar
- **[CallerLineNumber] int sourceLineNumber = 0**  - Não enviar
- **params ValueTuple<string, object>[] args** - Array de Tupple com (Nome, Objeto), o objeto será **serializado em Json** dentro do log
```
new (string, object)[]
{
    ("IsNewFlow", message.IsNewFlow),
    ("HasNewFlow", message.HasNewFlow),
    ("BusinessType", message.BusinessType)
});
```

## **Nota**:

Chamar sempre enviando o nome das propriedades que estão sendo enviadas, e não enviar nulos.

```
_logger.AddID("SimulationId", message.SimulationId);

_logger.LogCustom(LogLevel.Information,
        args: new (string, object)[]
        {
              ("IsNewFlow", message.IsNewFlow),
              ("HasNewFlow", message.HasNewFlow),
              ("BusinessType", message.BusinessType)
        });
```

Deploy
Automático em conjunto com qualquer Serviço que faça uso do RauchTech.Extensions.Logging
