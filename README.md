# vsHelp - Restaurador de Backups

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Windows Forms](https://img.shields.io/badge/Windows%20Forms-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![Google Drive](https://img.shields.io/badge/Google%20Drive-4285F4?style=for-the-badge&logo=google-drive&logoColor=white)
![WinRAR](https://img.shields.io/badge/WinRAR-F7931E?style=for-the-badge&logo=winrar&logoColor=white)

## 📝 Descrição do Projeto

O **vsHelp - Restaurador de Backups** é uma aplicação desktop desenvolvida em C# com Windows Forms, projetada para simplificar e automatizar o processo de restauração de backups. A ferramenta oferece uma interface intuitiva para gerenciar e restaurar arquivos de backup, com suporte a integração com o Google Drive para backups em nuvem e funcionalidades de descompactação via WinRAR.

Este projeto visa fornecer uma solução robusta e fácil de usar para garantir a recuperação eficiente de dados importantes, minimizando o tempo de inatividade e a perda de informações.

## ✨ Funcionalidades Principais

*   **Restauração de Backups:** Interface amigável para selecionar e restaurar backups de diversas fontes.
*   **Integração com Google Drive:** Suporte para baixar e restaurar backups armazenados diretamente no Google Drive.
*   **Descompactação WinRAR:** Capacidade de descompactar arquivos de backup compactados com WinRAR.
*   **Gerenciamento de Conexões:** Configuração e gerenciamento de conexões com bancos de dados para restauração de dados.
*   **Criptografia:** Funções de criptografia para garantir a segurança dos dados durante o processo de backup e restauração.
*   **Interface Gráfica Intuitiva:** Desenvolvido com Windows Forms para uma experiência de usuário simplificada.

## 🚀 Como Usar

### Pré-requisitos

*   Visual Studio 2022 ou superior
*   .NET Framework (versão compatível com o projeto, verificar `vsHelp.csproj`)
*   WinRAR instalado no sistema (para funcionalidades de descompactação)
*   Credenciais da API do Google Drive (`client_secrets.json`) configuradas para acesso à nuvem.

### Instalação e Execução

1.  **Clone o Repositório:**
    ```bash
    git clone https://github.com/seu-usuario/vsHelp.git
    cd vsHelp-4.0
    ```
    *(Nota: Substitua `https://github.com/seu-usuario/vsHelp.git` pelo URL real do seu repositório, se aplicável.)*

2.  **Abra no Visual Studio:**
    Abra o arquivo de solução `vsHelp.sln` no Visual Studio.

3.  **Restaure as Dependências:**
    O Visual Studio deve restaurar automaticamente as dependências NuGet. Caso contrário, clique com o botão direito na solução no Gerenciador de Soluções e selecione "Restaurar Pacotes NuGet".

4.  **Configure as Credenciais do Google Drive:**
    Certifique-se de que o arquivo `client_secrets.json` esteja configurado corretamente na raiz do projeto para a integração com o Google Drive.

5.  **Compile e Execute:**
    Compile o projeto (Ctrl+Shift+B) e execute-o (F5).

## 🤝 Contribuição

Contribuições são bem-vindas! Se você tiver sugestões de melhorias, relatar bugs ou quiser adicionar novas funcionalidades, sinta-se à vontade para abrir uma *issue* ou enviar um *pull request*.

## 📄 Licença

Este projeto está licenciado sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.