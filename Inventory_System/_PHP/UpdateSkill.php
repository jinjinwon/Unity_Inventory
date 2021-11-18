<?php
	$u_id = $_POST["Input_user"]; 
	$score = $_POST["SkillPoint"];
	$skill_list = $_POST["Skill_list"];

	$con = mysqli_connect("localhost", "jinone12", "wlsdnjs12!!", "jinone12");

	if(!$con)
		die("Could not Connect".mysqli_connect_error());
	$check = mysqli_query($con, "SELECT user_id FROM 3DPortfolio WHERE user_id ='".$u_id."'");

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{  		
		die("ID Does Exist.");
	} 
	if($row = mysqli_fetch_assoc($check))
	{
		mysqli_query($con, "UPDATE 3DPortfolio SET `SkillPoint`='".$score."' WHERE `user_id`='".$u_id."'");
		mysqli_query($con, "UPDATE 3DPortfolio SET `Skill_Info`='".$skill_list."' WHERE `user_id`='".$u_id."'");
		echo("UpDataSuccess");
	}
	mysqli_close($con);
?>