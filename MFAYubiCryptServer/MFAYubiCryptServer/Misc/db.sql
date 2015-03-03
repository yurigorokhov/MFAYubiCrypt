CREATE DATABASE yubicrypt;
USE yubicrypt;

CREATE TABLE IF NOT EXISTS users (
	user_id INT PRIMARY KEY AUTO_INCREMENT, 
    user_name VARCHAR(25),
	user_secret VARCHAR(80)
) ENGINE=INNODB;

CREATE TABLE IF NOT EXISTS encrypt_keys (
	keys_id INT PRIMARY KEY AUTO_INCREMENT,
	keys_encryption_id VARCHAR(80),
	keys_user_id INT(11),
	keys_key_encrypted VARCHAR(1024)
) ENGINE=INNODB;