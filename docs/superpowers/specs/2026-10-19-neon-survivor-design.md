# Neon Survivor — Design

Trabalho de faculdade, prazo 2026-10-19 23:59. Unity 6000.3, URP, Input System novo, 2D, 960x600.
Backup do projeto antigo (Platformer Microgame): tag git `backup-platformer`.

## Jogo
Top-down estilo Vampire Survivors, tema cyberpunk. Jogador só se move e dá dash; armas disparam sozinhas.
Objetivo: sobreviver 3:00 até a extração. Morrer = game over.

## Rubrica → features
- **Tempo:** cronômetro no HUD; dificuldade escala com o tempo (spawn rate, novos tipos em 0:45 e 1:30);
  boss aos 2:00; vitória aos 3:00; tela final mostra tempo sobrevivido + score.
- **Inimigos:** Drone (persegue), Moto kamikaze (linha reta em direção fixa, atravessa a arena),
  Torreta (parada, atira no jogador), Boss Mech (persegue + atira em leque).
- **Visual:** pixel art própria gerada via Python/PIL, paleta neon (rosa, ciano, roxo, amarelo).
- **Áudio:** música synthwave em loop + SFX (tiro, hit, morte inimigo, XP, level up, dash, dano, vitória/derrota), sintetizados.
- **UI:** barras de vida, stamina, XP; nível, score, tempo; painel de level up (3 opções); telas de game over / vitória.
- **Controles:** teclado (WASD/setas, Espaço dash, 1-3/Enter no level up) + gamepad (analógico, A dash, D-pad/A no menu).
- **Level design:** arena de rua neon fechada, obstáculos (carros, pilares), poças elétricas que causam dano.

## Armas (3 níveis cada)
1. Pistola de plasma — mira no inimigo mais próximo; upgrades: cadência, projéteis extras.
2. Lâmina monofio — lâminas orbitando; upgrades: +lâminas, +dano.
3. Pulso EMP — onda em área periódica com knockback; upgrades: raio, frequência.
Começa com a pistola. Level up oferece 3 opções entre armas novas/upgrades/vida/velocidade.

## Técnico
- Projeto limpo: remove assets do microgame; mantém Settings (URP), TextMesh Pro.
- Cena e objetos montados por script de Editor (`Tools > Build Neon Survivor`) — nunca editar YAML da cena à mão.
- Sprites e áudio em `Assets/Resources/`, carregados em runtime.
- Scripts em `Assets/Scripts/`, um por responsabilidade: GameManager, GameTimer/WaveDirector, Player, Weapons, Enemies, Pickups, HUD, AudioManager.
