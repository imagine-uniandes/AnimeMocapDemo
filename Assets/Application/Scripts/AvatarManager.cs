using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public GameObject[] Avatars;
    int avatarIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        SetAvatar();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            avatarIndex++;
            if (avatarIndex >= Avatars.Length)
                avatarIndex = 0;
            SetAvatar();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            avatarIndex--;
            if (avatarIndex < 0)
                avatarIndex = avatarIndex = Avatars.Length - 1;
            SetAvatar();
        }

    }

    private void SetAvatar()
    {
        foreach (GameObject avatar in Avatars)
            avatar.gameObject.SetActive(false);

        Avatars[avatarIndex].gameObject.SetActive(true);
        this.GetComponent<Manager>().animator = Avatars[avatarIndex].GetComponent<Animator>();
        this.GetComponent<MopSender>().animator = Avatars[avatarIndex].GetComponent<Animator>();
    }
}
