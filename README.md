# Trabalho Prático
Trabalho de Inteligência Artificial Aplicada a Jogos

# (Titulo do Jogo)

# Membros do Grupo:
- João Faria - 25590
- Samuel Fernandes - 31470

# Metodos de Aplicação de IA escolhidos
- Path Finding: Usado para que o Boss encontre o melhor caminho para ir ter ao jogador
- State Machine: Chamado através de ações feitas pelo Player, como por exemplo saltar, que faz com que o Player ative a animação de saltar, etc.
- Behavior Tree: Atribuido para o Boss ter mecânicas diferentes, quando o Player não está perto ele patrulha, se o Player está perto começa a focar nele, e se o Player está a atacar o Boss tenta fugir.

# Game Engine e Linguagem Escolhidas
- Game Engine: Unity
- Linguagem: C#

# Pastas Importantes
- Animations: Contem todas as animações do jogo
- Scene: Contem o cenário do jogo, sem ela o jogo não existe
- Scripts: Contem os scripts do jogo, ou seja o código do jogo

# Scripts
- Enemy
- EnemyBT
- EnemyHitbox
- PlayerAttack
- PlayerJump
- PlayerMovement
- PlayerMovementState

# Enemy
A classe Enemy atualiza a cada frame a orientação do inimigo lendo aiPath.desiredVelocity.x e ajustando transform.localScale para valores fixos (virar para a esquerda ou para a direita) conforme a direção do movimento; depende de um componente AIPath atribuído e não faz mais nada (pode dar NullReferenceException se aiPath estiver ausente), sendo comum substituir a escala fixa por SpriteRenderer.flipX ou adicionar verificação if (aiPath == null) return; como melhoria.

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public AIPath aiPath;

    void Update()
    {
        if (aiPath.desiredVelocity.x >= 0.01f){
            transform.localScale = new Vector3(-6f, 6f, 6f);
        } else if (aiPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(6f, 6f, 6f);
        }
    }
}
```

</details>

# EnemyBT
A classe EnemyBT trata-se da Behaviour Tree do inimigo, ou seja, o comportamento que este toma, através de certas ações do Player, ou ações já programadas nele, como o Patrol

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 

```

</details>

# EnemyHitbox
É a classe encarregada de calcular e atribuir a Hitbox do inimigo, para que o Player possa o atacar

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 

```

</details>

# PlayerAttack
Detecta quando o jogador pressiona E, dispara a animação de ataque e muda o estado de movimento para ataque; aplica dano aos inimigos dentro do alcance do ponto de ataque e notifica inimigos num raio próximo para reagirem; também desenha no editor uma indicação do alcance do ataque.

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 

```

</details>

# PlayerJump
Classe que trata toda a lógica de saltos do jogador:escuta a tecla de salto, determina se o jogador está no chão (raycast) ou encostado a uma parede e executa salto normal, duplo-salto ou wall‑jump conforme o caso; aplica forças ao Rigidbody2D, zera velocidades antes do salto quando necessário, controla um cooldown de movimento para wall‑jump e atualiza o PlayerMovementState para refletir o estado (Jump, Double_Jump, Wall_Jump). Também calcula dimensões do jogador (para raycasts) e garante que o duplo salto só ocorre uma vez até aterrissar.

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 

```

</details>

# PlayerMovement
Classe que trata o movimento horizontal do jogador: lê o eixo "Horizontal", aplica deslocamento multiplicado por speed via transform.Translate, atualiza a orientação do sprite (flip) comparando a posição atual com a do frame anterior e decrementa o wallJumpCooldown. Também calcula limites de ecrã e metade da largura do jogador no Start() e expõe animator/spriteRenderer para ligações no Inspector; nota que usa movimento por transformação direta em vez de física (Rigidbody2D).

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 

```

</details>

# PlayerMovementState
Classe que centraliza o estado de movimento do jogador: define o enum MoveState (Idle, Run, Attack, Jump, Fall, Double_Jump, Wall_Jump), determina o estado atual a cada frame com base na posição e na Rigidbody2D (velocidade vertical) e expõe SetMoveState para forçar transições. Para cada estado chama um handler que dispara a animação correspondente (usa o Animator) e notifica ouvintes via OnPlayerMoveStateChanged. Também tenta obter Rigidbody2D e Animator em Awake() se não estiverem atribuídos. Em resumo: liga a física/entrada ao sistema de animações e fornece um ponto único para mudar/consultar o estado de movimento.

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 

```

</details>

# Créditos
- Sprite do player: https://aamatniekss.itch.io/fantasy-knight-free-pixelart-animated-character
- Sprite do Boss: https://darkpixel-kronovi.itch.io/mecha-golem-free
- Sprite das plataformas: https://brackeysgames.itch.io/brackeys-platformer-bundle
- Script de pathfinding: https://arongranberg.com/astar/front
