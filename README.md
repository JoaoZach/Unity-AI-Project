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

# Créditos
- Sprite do player: https://aamatniekss.itch.io/fantasy-knight-free-pixelart-animated-character
- Sprite do Boss: https://darkpixel-kronovi.itch.io/mecha-golem-free
- Sprite das plataformas: https://brackeysgames.itch.io/brackeys-platformer-bundle
- Script de pathfinding: https://arongranberg.com/astar/front
