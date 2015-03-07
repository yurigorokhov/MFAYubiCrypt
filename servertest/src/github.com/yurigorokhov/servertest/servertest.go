package main

import (
	"bytes"
	uuid "code.google.com/p/go-uuid/uuid"
	"crypto"
	"crypto/hmac"
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"strconv"
	"strings"
)

type Connection struct {
	hostname string
	port     int
}

type User struct {
	UserName string
	Id       int
}

type Encryption struct {
	EncryptionId string
	ShaKeys      string
}

type Request struct {
	RequestId string
}

type Challenge struct {
	Id        string
	Challenge string
}

func main() {
	args := os.Args[1:]
	if len(args) != 3 {
		printErrorAndExit("USAGE: servertest host port numusers")
	}
	hostname := args[0]
	port, err := strconv.Atoi(args[1])
	if err != nil {
		printErrorAndExit("Could not parse port as number")
	}
	conn := Connection{hostname: hostname, port: port}
	numUsers, err := strconv.Atoi(args[2])
	if err != nil {
		printErrorAndExit("Could not parse numusers as number")
	}

	var usersById = make(map[int]*User)
	var secretsById = make(map[int]string)
	users := make([]*User, numUsers)

	// create random users
	for i := 0; i < numUsers; i++ {
		user, secret := createRandomUser(&conn)
		fmt.Printf("Creating user %v\n", user.UserName)
		usersById[user.Id] = &user
		secretsById[user.Id] = secret
		users[i] = &user
	}

	// set up an ecryption among all users
	encryption := setupEncryption(&conn, users)
	fmt.Printf("Set up encryption with id %v and sha of keys %v\n", encryption.EncryptionId, encryption.ShaKeys)

	// ask for decryption
	request := requestDecryption(&conn, encryption.EncryptionId)
	fmt.Printf("Asking to decrypt %v, got back request id %v\n", encryption.EncryptionId, request.RequestId)

	// foreach user ask & respond to a challenge
	for _, u := range users {
		challenge := getChallenge(&conn, u)
		fmt.Printf("Got challenge %v from user %v\n", challenge.Challenge, u.UserName)

		// compute HMAC-SHA1(challenge, secret)    this should be done by yubikey
		response := HMACSHA1(challenge.Challenge, secretsById[u.Id])

		// respond to challenge
		respondToChallenge(&conn, &challenge, response)
		fmt.Printf("Responding to challenge with HMAC-SHA1(challenge, user-secret)\n")
	}

	// check that our harddrive was properly decrypted
	response := checkDecryptionRequest(&conn, &request)
	fmt.Printf("Received decryption key %v\n", response)
	if response != encryption.ShaKeys {
		fmt.Printf("The received key does not match ShaKeys: %v", encryption.ShaKeys)
	} else {
		fmt.Printf("The received keys match! authentication successful")
	}
}

// Create a random user
func createRandomUser(conn *Connection) (user User, secret string) {
	name := genRandomString()
	secret = genRandomString()
	req, err := http.NewRequest(
		"POST",
		fmt.Sprintf("http://%v:%v/api/users?username=%v&secret=%v", conn.hostname, conn.port, name, secret),
		new(bytes.Buffer))
	if err != nil {
		panic(err)
	}
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()
	decoder := json.NewDecoder(resp.Body)
	var u User
	err = decoder.Decode(&u)
	if err != nil {
		panic(err)
	}
	return u, secret
}

func setupEncryption(conn *Connection, users []*User) (encryption Encryption) {
	userNames := make([]string, len(users), len(users))
	for i, u := range users {
		userNames[i] = u.UserName
	}
	req, err := http.NewRequest(
		"POST",
		fmt.Sprintf("http://%v:%v/api/setup?users=%v", conn.hostname, conn.port, strings.Join(userNames, ",")),
		new(bytes.Buffer))
	if err != nil {
		panic(err)
	}
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()
	decoder := json.NewDecoder(resp.Body)
	var e Encryption
	err = decoder.Decode(&e)
	if err != nil {
		panic(err)
	}
	return e
}

func requestDecryption(conn *Connection, encryptionId string) Request {
	req, err := http.NewRequest(
		"POST",
		fmt.Sprintf("http://%v:%v/api/decrypt/%v", conn.hostname, conn.port, encryptionId),
		new(bytes.Buffer))
	if err != nil {
		panic(err)
	}
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()
	decoder := json.NewDecoder(resp.Body)
	var r Request
	err = decoder.Decode(&r)
	if err != nil {
		panic(err)
	}
	return r
}

func getChallenge(conn *Connection, user *User) Challenge {
	req, err := http.NewRequest(
		"GET",
		fmt.Sprintf("http://%v:%v/api/challenge/%v", conn.hostname, conn.port, user.Id),
		new(bytes.Buffer))
	if err != nil {
		panic(err)
	}
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()
	decoder := json.NewDecoder(resp.Body)
	var c Challenge
	err = decoder.Decode(&c)
	if err != nil {
		panic(err)
	}
	return c
}

func respondToChallenge(conn *Connection, challenge *Challenge, response string) {
	req, err := http.NewRequest(
		"POST",
		fmt.Sprintf("http://%v:%v/api/challenge/%v", conn.hostname, conn.port, challenge.Id),
		bytes.NewBufferString(response))
	if err != nil {
		panic(err)
	}
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	resp.Body.Close()
}

func checkDecryptionRequest(conn *Connection, request *Request) string {
	req, err := http.NewRequest(
		"GET",
		fmt.Sprintf("http://%v:%v/api/decrypt/%v", conn.hostname, conn.port, request.RequestId),
		new(bytes.Buffer))
	if err != nil {
		panic(err)
	}
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()
	buf := new(bytes.Buffer)
	buf.ReadFrom(resp.Body)
	return buf.String()
}

/* Misc helper functions */
func HMACSHA1(value string, secret string) string {
	mac := hmac.New(crypto.SHA1.New, []byte(secret))
	mac.Write([]byte(value))
	return string(mac.Sum(nil))
}

func genRandomString() string {
	return uuid.NewRandom().String()
}

func printErrorAndExit(msg string) {
	fmt.Println(msg)
	os.Exit(1)
}
