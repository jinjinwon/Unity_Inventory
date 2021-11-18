<?php
	$u_id = $_POST["Input_user"]; 
	$u_pw = $_POST["Input_pass"];

	$con = mysqli_connect("localhost", "jinone12", "wlsdnjs12!!", "jinone12");
	// "localhost" <-- 같은 서버 내

	if(!$con)
		die( "Could not Connect" . mysqli_connect_error()); 
	//연결 실패했을 경우 이 스크립트를 닫아주겠다는 뜻	

	$check = mysqli_query($con, "SELECT * FROM 3DPortfolio WHERE user_id ='".$u_id."'");

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{  //mysqli_num_rows() 함수는 데이터베이스에서 쿼리를 보내서 나온 레코드의 개수를 알아낼 때 쓰임
	   //즉 0이라는 뜻은 해당 조건을 못 찾았다는 뜻
		
		die("ID Does Exist.");
	}

	$row = mysqli_fetch_assoc($check); //user_id 이름에 해당하는 행의 내용을 가져온다.
	if($row)
	{
		if($u_pw == $row["user_pw"])
		{	
			// JSON 생성을 위한 변수
			$RowDatas = array();
			$RowDatas["nick_name"] = $row["nick_name"];
			$RowDatas["my_gold"] = $row["my_gold"];
			$RowDatas["SkillPoint"] = $row["SkillPoint"];
			$output = json_encode($RowDatas,JSON_UNESCAPED_UNICODE);

			// 출력
			echo $output;
			echo "<br>";
			echo "Login-Success";

		}
		else
		{
			die("Pass does not Match. \n");
		}
	}

	mysqli_close($con);
?>