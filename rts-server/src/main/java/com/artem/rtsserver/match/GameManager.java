package com.artem.rtsserver.match;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Random;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.artem.rtsserver.database.MatchHistoryDAO;
import com.artem.rtsserver.database.PlayerDAO;
import com.artem.rtsserver.lobby.LobbyPlayer;
import com.artem.rtsserver.net.server.ClientConnection;

@Component
public class GameManager {

	private final Map<String, GameSession> matchesById = new HashMap<>();
	private final Map<Integer, String> matchIdByPlayerId = new HashMap<>();

	private final MatchHistoryDAO matchHistoryDAO;
	private final PlayerDAO playerDAO;

	@Autowired
	public GameManager(PlayerDAO playerDAO, MatchHistoryDAO matchHistoryDAO) {
		this.playerDAO = playerDAO;
		this.matchHistoryDAO = matchHistoryDAO;
	}

	public String createMatch(List<LobbyPlayer> players) {
		Random randomNumber = new Random();
		String matchId;

		do {
			int id = randomNumber.nextInt(900000) + 10000;
			matchId = String.valueOf(id);
		} while (matchesById.containsKey(matchId));

		GameSession session = new GameSession(matchId, players, this, playerDAO, matchHistoryDAO);
		matchesById.put(matchId, session);

		for (LobbyPlayer player : players) {
			matchIdByPlayerId.put(player.getPlayerId(), matchId);
		}

		session.start();
		return matchId;
	}

	public GameSession getMatchSession(String matchId) {
		return matchesById.get(matchId);
	}

	public void endMatchSession(String matchId) {
		GameSession session = matchesById.remove(matchId);

		if (session == null)
			return;

		session.stop();

		for (LobbyPlayer player : session.getPlayers()) {
			matchIdByPlayerId.remove(player.getPlayerId());
			player.getConn().clearMatchId();
		}

		System.out.println("MATCH ENDED: " + matchId);
	}

	public GameSession getMatchByPlayer(int playerId) {
		String matchId = matchIdByPlayerId.get(playerId);
		if (matchId == null)
			return null;
		return matchesById.get(matchId);
	}

	public String createAIMatch(ClientConnection client, String difficultyText) {

		Random randomNumber = new Random();

		String matchId;

		do {

			int id = randomNumber.nextInt(900000) + 10000;

			matchId = String.valueOf(id);

		} while (matchesById.containsKey(matchId));

		AIController.Difficulty difficulty = AIController.Difficulty.NORMAL;

		if ("easy".equalsIgnoreCase(difficultyText)) {

			difficulty = AIController.Difficulty.EASY;
		}

		if ("hard".equalsIgnoreCase(difficultyText)) {

			difficulty = AIController.Difficulty.HARD;
		}

			LobbyPlayer player = new LobbyPlayer(client);
		List<LobbyPlayer> players = List.of(player);

		GameSession session = new GameSession(matchId, players, this, playerDAO, matchHistoryDAO, difficulty);

		matchesById.put(matchId, session);
		matchIdByPlayerId.put(client.getPlayerId(), matchId);

		client.setMatchId(matchId);
		session.start();

		System.out.println("AI MATCH CREATED id=" + matchId + " difficulty=" + difficulty);

		return matchId;
	}

}